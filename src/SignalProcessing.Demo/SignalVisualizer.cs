namespace SignalProcessing.Demo;

public static class SignalVisualizer
{
    public static void PlotSignal(double[] samples, string title, int width = 80, int height = 20)
    {
        Console.WriteLine($"\n{title}");
        Console.WriteLine(new string('=', width));

        if (samples.Length == 0)
        {
            Console.WriteLine("No samples to display");
            return;
        }

        // Find min and max values
        var min = samples.Min();
        var max = samples.Max();
        var range = max - min;

        if (range == 0)
        {
            Console.WriteLine("All samples have the same value");
            return;
        }

        // Downsample if needed
        var displaySamples = samples;
        if (samples.Length > width)
        {
            displaySamples = DownsampleSignal(samples, width);
        }

        // Create the plot
        for (int row = 0; row < height; row++)
        {
            var threshold = max - (row * range / (height - 1));
            
            for (int col = 0; col < displaySamples.Length; col++)
            {
                var value = displaySamples[col];
                
                // Determine character to display
                if (row == 0 && value >= threshold)
                    Console.Write('▄');
                else if (row == height - 1 && value <= threshold)
                    Console.Write('▀');
                else if (Math.Abs(value - threshold) < range / (height * 2))
                    Console.Write('█');
                else if (value > threshold)
                    Console.Write('│');
                else
                    Console.Write(' ');
            }
            
            // Show scale on the right
            Console.WriteLine($" {threshold:F3}");
        }

        Console.WriteLine(new string('─', width));
        Console.WriteLine($"Samples: {samples.Length} | Min: {min:F3} | Max: {max:F3} | Range: {range:F3}");
    }

    private static double[] DownsampleSignal(double[] samples, int targetLength)
    {
        var result = new double[targetLength];
        var step = (double)samples.Length / targetLength;

        for (int i = 0; i < targetLength; i++)
        {
            var index = (int)(i * step);
            result[i] = samples[index];
        }

        return result;
    }
}
