// See https://aka.ms/new-console-template for more information

using GameSpace;


class Program 
{
	public static void Main()
	{
		Console.WriteLine("Starting Game Window...");
		Game gameWindow = new Game(600, 600, "Basic Terrain Gen");
		using (gameWindow)
		{
			gameWindow.Run();
		}

	}

}

