using SeleniumLab.Task1;
using SeleniumLab.Task2;

Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║     Selenium Lab  —  demoqa.com  (C#)        ║");
Console.WriteLine("╚══════════════════════════════════════════════╝\n");

// ── TASK 1: Element finding demo ──────────────────────────────────────────────
new ElementFindingDemo().Run();

Console.WriteLine("\n" + new string('─', 50) + "\n");

// ── TASK 2: Automated tests ───────────────────────────────────────────────────
new TestRunner().RunAll();
