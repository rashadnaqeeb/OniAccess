// A slider widget (KSlider). Speaks formatted value, validates
// interactability, adjusts value with step math and boundary clamping.

namespace OniAccess.Widgets

class SliderWidget : Widget (line 6)
  override string GetSpeechText() (line 7)
    // SpeechFunc short-circuits if set. Otherwise returns "{Label}, {WidgetOps.FormatSliderValue(slider)}".
    // Falls back to Label if Component is not a KSlider.
  override bool IsValid() (line 18)
    // Returns false if inactive in hierarchy. Checks slider.interactable.
    // Falls back to Component/GameObject non-null check.
  override bool IsAdjustable (line 25)
    // Always returns true.
  override bool Adjust(int direction, int stepLevel) (line 31)
    // Computes step size: for whole-number sliders uses InputUtil.StepForLevel(stepLevel);
    // for float sliders uses range * InputUtil.FractionForLevel(stepLevel).
    // Clamps new value to [minValue, maxValue]. Returns true if value changed.
  string GetBoundarySound(int direction) (line 55)
    // Returns "Slider_Boundary_Low" when at minimum moving down,
    // "Slider_Boundary_High" when at maximum moving up,
    // "Slider_Move" otherwise. Returns null if Component is not a KSlider.
