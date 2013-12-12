SA-MP-Server-Query-Class
========================

Basic Samp Server Query API for C#


1. Add the class to your project.
2. using SampQueryApi; // This should be on-top of the file you're using.
3. Have fun!

You may hack it, modify it, repost it but please do not sell the code or lose the credits. That's all.

Here's an example (C#) - It's very simple:
```
static void Main(string[] args)
{
    SampQuery api = new SampQuery("66.85.149.2", 7777, 'i');

    foreach (KeyValuePair<string, string> kvp in api.read(true))
    {
        Console.WriteLine("{0}: {1}",
            kvp.Key, kvp.Value);
    }

    Console.ReadLine(); // So we can close the console when we hit ENTER.
}
```
