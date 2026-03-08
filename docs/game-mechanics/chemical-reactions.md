# Chemical Reactions

How elements interact with each other in the simulation. Derived from decompiled source code.

## ElementInteraction System

The simulation supports element interactions via the `ElementInteraction` system:

```
struct ElementInteraction {
    interactionType              // Reaction type
    elemIdx1, elemIdx2           // Reactant elements
    elemResultIdx                // Product element
    minMass                      // Minimum mass threshold for both reactants
    interactionProbability       // 0.0-1.0 chance per frame
    elem1MassDestructionPercent  // Fraction of element 1 consumed
    elem2MassRequiredMultiplier  // How much element 2 needed relative to element 1
    elemResultMassCreationMultiplier  // Output mass multiplier
}
```

When two elements meet in adjacent cells with sufficient mass, they react probabilistically. Reactions are registered at game startup via `SimMessages.CreateElementInteractions()`.

## Known Reaction Types

Three reaction types are defined in `ElementInteractionHashes.cs`:

| Type | Hash | Description |
|------|------|-------------|
| GasObliteration | 1747383933 | Gas element is destroyed on contact |
| LiquidObliteration | 764054208 | Liquid element is destroyed on contact |
| LiquidConversion | 1537054354 | Liquid element converts to another element |

The specific element pairings and parameter values for these reactions are configured in the native C++ sim (SimDLL.dll) and are not visible in the decompiled C# source.
