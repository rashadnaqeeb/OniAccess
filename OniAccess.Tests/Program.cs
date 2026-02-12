using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using OniAccess.Input;

namespace OniAccess.Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            // Resolve game assemblies at runtime from ONI_MANAGED
            var managed = Environment.GetEnvironmentVariable("ONI_MANAGED")
                ?? @"C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed";

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                var name = new AssemblyName(e.Name).Name;
                var path = Path.Combine(managed, name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };

            return RunTests();
        }

        static int RunTests()
        {
            var results = new List<(string name, bool passed, string detail)>();

            results.Add(ActiveHandlerIsTop());
            results.Add(CapturesAllInputReadable());
            results.Add(PopExposesLowerHandler());
            results.Add(PushCallsOnActivate());
            results.Add(PopCallsOnDeactivate());
            results.Add(PopReactivatesExposedHandler());
            results.Add(ReplaceSwapsHandlers());
            results.Add(ClearEmptiesStack());
            results.Add(DeactivateAllCallsOnDeactivate());

            HandlerStack.Clear();

            int passed = 0, failed = 0;
            foreach (var (name, ok, detail) in results)
            {
                if (ok)
                {
                    Console.WriteLine($"  PASS  {name}");
                    passed++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  FAIL  {name}: {detail}");
                    Console.ResetColor();
                    failed++;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"{passed} passed, {failed} failed, {results.Count} total");

            return failed > 0 ? 1 : 0;
        }

        // --- Test handler ---

        private class TestHandler : IAccessHandler
        {
            public string DisplayName { get; }
            public bool CapturesAllInput { get; }
            public IReadOnlyList<HelpEntry> HelpEntries { get; }
                = new List<HelpEntry>().AsReadOnly();

            public int ActivateCount { get; private set; }
            public int DeactivateCount { get; private set; }

            public TestHandler(string name, bool capturesAll = false)
            {
                DisplayName = name;
                CapturesAllInput = capturesAll;
            }

            public void Tick() { }
            public bool HandleKeyDown(KButtonEvent e) => false;
            public void OnActivate() => ActivateCount++;
            public void OnDeactivate() => DeactivateCount++;
        }

        // --- Helpers ---

        private static void Reset() => HandlerStack.Clear();

        private static (string, bool, string) Assert(string name, bool ok, string detail)
            => (name, ok, ok ? "OK" : detail);

        // --- Tests ---

        private static (string, bool, string) ActiveHandlerIsTop()
        {
            Reset();
            var first = new TestHandler("First");
            var second = new TestHandler("Second");
            HandlerStack.Push(first);
            HandlerStack.Push(second);

            bool ok = HandlerStack.ActiveHandler == second;
            return Assert("ActiveHandlerIsTop", ok,
                $"expected Second, got {HandlerStack.ActiveHandler?.DisplayName ?? "null"}");
        }

        private static (string, bool, string) CapturesAllInputReadable()
        {
            Reset();
            var handler = new TestHandler("Modal", capturesAll: true);
            HandlerStack.Push(handler);

            bool ok = HandlerStack.ActiveHandler.CapturesAllInput;
            return Assert("CapturesAllInputReadable", ok, "CapturesAllInput was false");
        }

        private static (string, bool, string) PopExposesLowerHandler()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var top = new TestHandler("Top");
            HandlerStack.Push(bottom);
            HandlerStack.Push(top);
            HandlerStack.Pop();

            bool ok = HandlerStack.ActiveHandler == bottom && HandlerStack.Count == 1;
            return Assert("PopExposesLowerHandler", ok,
                $"ActiveHandler={HandlerStack.ActiveHandler?.DisplayName ?? "null"}, count={HandlerStack.Count}");
        }

        private static (string, bool, string) PushCallsOnActivate()
        {
            Reset();
            var handler = new TestHandler("Test");
            HandlerStack.Push(handler);

            bool ok = handler.ActivateCount == 1;
            return Assert("PushCallsOnActivate", ok,
                $"ActivateCount={handler.ActivateCount}");
        }

        private static (string, bool, string) PopCallsOnDeactivate()
        {
            Reset();
            var handler = new TestHandler("Test");
            HandlerStack.Push(handler);
            HandlerStack.Pop();

            bool ok = handler.DeactivateCount == 1;
            return Assert("PopCallsOnDeactivate", ok,
                $"DeactivateCount={handler.DeactivateCount}");
        }

        private static (string, bool, string) PopReactivatesExposedHandler()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var top = new TestHandler("Top");
            HandlerStack.Push(bottom); // ActivateCount = 1
            HandlerStack.Push(top);
            HandlerStack.Pop(); // should reactivate bottom

            bool ok = bottom.ActivateCount == 2;
            return Assert("PopReactivatesExposedHandler", ok,
                $"ActivateCount={bottom.ActivateCount}, expected 2");
        }

        private static (string, bool, string) ReplaceSwapsHandlers()
        {
            Reset();
            var first = new TestHandler("First");
            var second = new TestHandler("Second");
            HandlerStack.Push(first);
            HandlerStack.Replace(second);

            bool ok = HandlerStack.ActiveHandler == second
                   && HandlerStack.Count == 1
                   && first.DeactivateCount == 1
                   && second.ActivateCount == 1;
            return Assert("ReplaceSwapsHandlers", ok,
                $"Active={HandlerStack.ActiveHandler?.DisplayName ?? "null"}, " +
                $"count={HandlerStack.Count}, " +
                $"first.Deactivate={first.DeactivateCount}, " +
                $"second.Activate={second.ActivateCount}");
        }

        private static (string, bool, string) ClearEmptiesStack()
        {
            Reset();
            HandlerStack.Push(new TestHandler("A"));
            HandlerStack.Push(new TestHandler("B"));
            HandlerStack.Clear();

            bool ok = HandlerStack.Count == 0 && HandlerStack.ActiveHandler == null;
            return Assert("ClearEmptiesStack", ok,
                $"count={HandlerStack.Count}");
        }

        private static (string, bool, string) DeactivateAllCallsOnDeactivate()
        {
            Reset();
            var bottom = new TestHandler("Bottom");
            var top = new TestHandler("Top");
            HandlerStack.Push(bottom);
            HandlerStack.Push(top);
            HandlerStack.DeactivateAll();

            // Only the active (top) handler gets OnDeactivate
            bool ok = top.DeactivateCount == 1
                   && bottom.DeactivateCount == 0
                   && HandlerStack.Count == 0;
            return Assert("DeactivateAllCallsOnDeactivate", ok,
                $"top.Deactivate={top.DeactivateCount}, " +
                $"bottom.Deactivate={bottom.DeactivateCount}, " +
                $"count={HandlerStack.Count}");
        }
    }
}
