namespace AgilitySportsAPI.Utilities;

public class ColorWheel : IColorWheel
{
    List<string> colorWheel;
    private int currentColor = 0;

    public string Next()
    {
        if (currentColor >= colorWheel.Count - 1)
            currentColor = 0;
        else
            currentColor += 1;

        return colorWheel[currentColor];

    }
    public ColorWheel()
    {
        colorWheel = new List<string>
        {
            "#3f76bf",  // Blue Crayola
            "#4d7071",  // Slate Blue
            "#003c8e",  // Dark Blue
            "#5691af",  // light blue
            "#4292ff",  // Blue (Pantone)
            "#8c9dad",  // Cadet Blue
            "#b7c0cc",  // Blue Grey            
            "#3B82F",   // blue
            "#99c5b5",  // Green Blue Crayola
            "#1e7dff",  // Dodger Blue
            "#d7dfe6",  // Light Steel Blue
            "#6d8891",  // Steel Blue
            "#23426b",  // Dark Blue Grey
            "#eaeff5",  // Light Blue Grey
            "#437ab7",  // sea blue
        };
    }
}