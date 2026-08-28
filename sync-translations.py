#!/usr/bin/env python3
"""Sync translations/*.po with the mod's strings_template.pot.

The game regenerates mods/strings_templates/strings_template.pot every time
the mod loads (ModUtil.RegisterForTranslation). This script copies that file
into the repo and rewrites every .po so that it has exactly the template's
keys, in the template's order:

- keys new in the template are added with an empty msgstr (the game then
  falls back to English for them);
- keys whose English msgid changed get their msgstr blanked, with the old
  msgid and old translation kept as "#| " comments until retranslated,
  because the game would otherwise speak the stale translation;
- keys the template no longer has are dropped.

Usage:
  python3 sync-translations.py            # sync, report what needs translating
  python3 sync-translations.py --check    # report only, exit 1 if out of sync
  python3 sync-translations.py --pot PATH # use this template instead of the game's
"""

import argparse
import os
import platform
import re
import sys
from datetime import date
from pathlib import Path

REPO = Path(__file__).resolve().parent
REPO_POT = REPO / "strings_template.pot"
PO_DIR = REPO / "translations"
POT_RELATIVE = Path("mods") / "strings_templates" / "strings_template.pot"


def game_pot_candidates():
    system = platform.system()
    if system == "Darwin":
        yield Path.home() / "Library" / "Application Support" / "unity.Klei.Oxygen Not Included" / POT_RELATIVE
    elif system == "Windows":
        try:
            import winreg
            key = r"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders"
            with winreg.OpenKey(winreg.HKEY_CURRENT_USER, key) as k:
                docs = Path(os.path.expandvars(winreg.QueryValueEx(k, "Personal")[0]))
        except OSError as ex:
            print(f"warning: could not read the Documents folder from the registry: {ex}")
            docs = Path.home() / "Documents"
        yield docs / "Klei" / "OxygenNotIncluded" / POT_RELATIVE


class Entry:
    __slots__ = ("ctxt", "msgid", "msgstr", "previous")

    def __init__(self, ctxt, msgid, msgstr, previous):
        self.ctxt = ctxt
        self.msgid = msgid
        self.msgstr = msgstr
        self.previous = previous  # "#| " lines, kept only while untranslated


QUOTED = re.compile(r'^(msgctxt|msgid|msgstr) "(.*)"$')


def parse(text):
    """Return (header_lines, entries). Header is everything up to the first '#.' line.

    Strings are kept in their escaped on-disk form; the game writes one line per
    field, and continuation lines are concatenated so hand-wrapped files still parse.
    """
    lines = text.split("\n")
    first = next((i for i, l in enumerate(lines) if l.startswith("#.")), len(lines))
    header = lines[:first]
    while header and header[-1] == "":
        header.pop()
    entries = []
    block = []

    def flush():
        if not block:
            return
        fields = {}
        previous = []
        current = None
        for line in block:
            if line.startswith("#|"):
                previous.append(line)
            elif line.startswith("#"):
                current = None
            elif line.startswith('"') and current:
                fields[current] += line[1:-1]
            else:
                m = QUOTED.match(line)
                if not m:
                    raise ValueError(f"unrecognised line in entry block: {line!r}")
                current = m.group(1)
                fields[current] = m.group(2)
        if "msgctxt" not in fields:
            raise ValueError(f"entry without msgctxt: {block[0]!r}")
        entries.append(Entry(fields["msgctxt"], fields.get("msgid", ""), fields.get("msgstr", ""), previous))
        block.clear()

    for line in lines[first:]:
        if line == "":
            flush()
        else:
            block.append(line)
    flush()
    return header, entries


def render(header, entries):
    out = list(header)
    out.append("")
    for e in entries:
        out.append(f"#. {e.ctxt}")
        if not e.msgstr:
            out.extend(e.previous)
        out.append(f'msgctxt "{e.ctxt}"')
        out.append(f'msgid "{e.msgid}"')
        out.append(f'msgstr "{e.msgstr}"')
        out.append("")
    return "\n".join(out)


def read(path):
    raw = path.read_bytes().decode("utf-8")
    newline = "\r\n" if "\r\n" in raw else "\n"
    return raw.replace("\r\n", "\n"), newline


def write(path, text, newline):
    path.write_bytes(text.replace("\n", newline).encode("utf-8"))


def update_pot(pot_arg, check):
    """Bring the repo's strings_template.pot up to date.

    Returns (changed, template_text); the text is the up-to-date template even in check mode.
    """
    if pot_arg:
        source = Path(pot_arg)
        if not source.is_file():
            sys.exit(f"error: {source} does not exist")
    else:
        source = next((p for p in game_pot_candidates() if p.is_file()), None)
        if source is None:
            print("note: no game-generated template found; using the repo's strings_template.pot")
            print("      (build, launch the game once, then rerun to pick up new strings)")
            return False, read(REPO_POT)[0]
    new_text, _ = read(source)
    if REPO_POT.is_file():
        old_text, newline = read(REPO_POT)
    else:
        old_text, newline = None, "\n"
    if new_text == old_text:
        print(f"template unchanged: {source}")
        return False, new_text
    if check:
        print(f"template differs from {source}")
    else:
        write(REPO_POT, new_text, newline)
        print(f"template updated from {source}")
    return True, new_text


def sync_po(path, template, check):
    """Returns (changed, added, changed_source, removed, untranslated) for one .po."""
    text, newline = read(path)
    header, entries = parse(text)
    old = {e.ctxt: e for e in entries}
    added, stale, untranslated = [], [], []
    removed = [e.ctxt for e in entries if e.ctxt not in template]
    new_entries = []
    for t in template.values():
        e = old.get(t.ctxt)
        if e is None:
            e = Entry(t.ctxt, t.msgid, "", [])
            added.append(t.ctxt)
        elif e.msgid != t.msgid:
            previous = [f'#| msgid "{e.msgid}"', f'#| msgstr "{e.msgstr}"'] if e.msgstr else e.previous
            e = Entry(t.ctxt, t.msgid, "", previous)
            stale.append(t.ctxt)
        if not e.msgstr:
            untranslated.append(e.ctxt)
        new_entries.append(e)

    new_text = render(header, new_entries)
    changed = new_text != text
    if changed and not check:
        stamped = [f'"PO-Revision-Date: {date.today().isoformat()}\\n"' if h.startswith('"PO-Revision-Date: ') else h
                   for h in header]
        write(path, render(stamped, new_entries), newline)
        verify_text, _ = read(path)
        _, verify_entries = parse(verify_text)
        assert [e.ctxt for e in verify_entries] == list(template), f"{path.name}: key order does not match the template after writing"
        assert all(e.msgid == template[e.ctxt].msgid for e in verify_entries), f"{path.name}: msgid mismatch after writing"
    return changed, added, stale, removed, untranslated


def main():
    ap = argparse.ArgumentParser(description=__doc__.split("\n\n")[0])
    ap.add_argument("--pot", help="template to sync from instead of the game's generated one")
    ap.add_argument("--check", action="store_true", help="report only; exit 1 if anything is out of sync")
    args = ap.parse_args()

    out_of_sync, pot_text = update_pot(args.pot, args.check)
    _, pot_entries = parse(pot_text)
    template = {e.ctxt: e for e in pot_entries}
    if len(template) != len(pot_entries):
        sys.exit("error: duplicate msgctxt in the template")
    print(f"template has {len(template)} strings")

    po_files = sorted(PO_DIR.glob("*.po"))
    if not po_files:
        sys.exit(f"error: no .po files in {PO_DIR}")

    needs_translation = {}
    for path in po_files:
        changed, added, stale, removed, untranslated = sync_po(path, template, args.check)
        out_of_sync |= changed
        status = "out of sync" if (changed and args.check) else ("updated" if changed else "in sync")
        print(f"{path.name}: {status}, {len(added)} added, {len(stale)} source changed, {len(removed)} removed, {len(untranslated)} untranslated")
        for ctxt in removed:
            print(f"  removed: {ctxt}")
        for ctxt in untranslated:
            needs_translation.setdefault(ctxt, []).append(path.stem)

    if needs_translation:
        print(f"\n{len(needs_translation)} string(s) need translation (empty msgstr):")
        for ctxt, langs in needs_translation.items():
            print(f"  {ctxt} [{', '.join(langs)}]: {template[ctxt].msgid}")

    if args.check and out_of_sync:
        sys.exit(1)


if __name__ == "__main__":
    main()
