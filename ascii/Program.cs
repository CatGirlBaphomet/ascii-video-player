using OpenCvSharp;
using System;
using System.Text;

class Program
{
    // ASCII characters to use for different grayscale levels
    const string ASCII_CHARS = "@%#*+=-:. ";

    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Please provide the path to the video file to play");
            return;
        }

        PlayVideoInTerminal(args[0]);
    }

    static void PlayVideoInTerminal(string videoPath)
    {
        Console.WriteLine("Choose a resolution:");
        Console.WriteLine("1. Full resolution (slower)");
        Console.WriteLine("2. Limited resolution (faster)");
        string input = Console.ReadLine();
        bool fullResolution = input == "1";
    
        using (var capture = new VideoCapture(videoPath))
        {
            Mat frame = new Mat();
            double fps = capture.Fps; // Get the frame rate of the video
            int delay = (int)(1000 / fps); // Calculate the delay in milliseconds
    
            // Get the original video size and aspect ratio
            int originalWidth = capture.FrameWidth;
            int originalHeight = capture.FrameHeight;
    
            while (true)
            {
                capture.Read(frame);
                if (frame.Empty())
                    break;
    
                // Get the size of the terminal window
                int windowWidth = Console.WindowWidth;
                int windowHeight = Console.WindowHeight;
    
                // Calculate the new size
                int newWidth, newHeight;
                if (fullResolution)
                {
                    newWidth = windowWidth;
                    newHeight = windowHeight;
                    Cv2.Resize(frame, frame, new OpenCvSharp.Size(newWidth, newHeight));
                }
                else
                {
                    newWidth = Math.Min(windowWidth, originalWidth);
                    newHeight = Math.Min(windowHeight, originalHeight);
                    Cv2.Resize(frame, frame, new OpenCvSharp.Size(newWidth, newHeight));
                }
    
                string asciiFrame = ColorImageToAscii(frame);
    
                // Overwrite the old frame with whitespace
                Console.SetCursorPosition(0, 0); // Reset the cursor position
                for (int i = 0; i < windowHeight; i++)
                {
                    Console.WriteLine(new string(' ', windowWidth));
                }
    
                // Write the new frame
                Console.SetCursorPosition(0, 0); // Reset the cursor position
                Console.WriteLine(asciiFrame);
    
                Thread.Sleep(delay); // Add a delay
            }
        }
    }
    static string ColorImageToAscii(Mat image)
    {
        StringBuilder ascii = new StringBuilder();
        string asciiChars = "@%#*+=-:. ";

        for (int y = 0; y < image.Rows; y++)
        {
            for (int x = 0; x < image.Cols; x++)
            {
                Vec3b color = image.At<Vec3b>(y, x);

                // Convert the color to an ANSI escape code
                string ansiColor = $"\x1b[38;2;{color[2]};{color[1]};{color[0]}m";

                // Choose an ASCII character based on the brightness
                int brightness = (int)(0.3 * color[2] + 0.59 * color[1] + 0.11 * color[0]);
                int index = brightness * asciiChars.Length / 256;
                char asciiChar = asciiChars[index];

                ascii.Append(ansiColor + asciiChar);
            }

            ascii.AppendLine();
        }

        // Reset the color
        ascii.Append("\x1b[0m");

        return ascii.ToString();
    }

    static string GrayscaleImageToAscii(Mat image)
    {
        StringBuilder asciiStr = new StringBuilder();
        for (int i = 0; i < image.Rows; i++)
        {
            for (int j = 0; j < image.Cols; j++)
            {
                byte color = image.At<byte>(i, j);
                int index = color * ASCII_CHARS.Length / 256;
                asciiStr.Append(ASCII_CHARS[index]);
            }
            asciiStr.AppendLine();
        }
        return asciiStr.ToString();
    }
}