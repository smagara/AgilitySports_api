namespace AgilitySportsAPI.Utilities;

public class ColorWheel
{
    List<string> colorWheel;
    private int currentColor = 0;

    public string Next()
    {
        if (currentColor >= colorWheel.Count-1)
            currentColor = 0;
        else
            currentColor += 1;

        return colorWheel[currentColor];

    }
    public ColorWheel()
    {
        colorWheel = new List<string>
        {
            "#1c9ea6",
            "#31e3c4",
            "#72a7a8",
            "#ffa700",
            "#7fffd4",
            "#af6f09",
            "#4d7071",
            "#722f37",
            "#f4a460",
            "#ab6819",
            "#c7c10c"
        };
    }
}