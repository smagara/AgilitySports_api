# Copilot conversation, refactoring the program.cs endpoint definitions

**Copilot prompt:**
@workspace The main program.cs code file is getting cluttered with endpoint definitions. What are best practices for this structure to increase readability and ease future maintenance.


**Reply:**
To improve the readability and maintainability of your [Program.cs](vscode-file://vscode-app/c:/Users/smaga/AppData/Local/Programs/Microsoft%20VS%20Code/resources/app/out/vs/code/electron-sandbox/workbench/workbench.html) file, you can refactor the endpoint definitions by organizing them into separate files or classes. Here are some best practices to achieve this:

- Use Extension Methods: Create extension methods for each group of endpoints. This allows you to keep related endpoints together and makes the [Program.cs](vscode-file://vscode-app/c:/Users/smaga/AppData/Local/Programs/Microsoft%20VS%20Code/resources/app/out/vs/code/electron-sandbox/workbench/workbench.html) file cleaner.

- [Choosing this option] Organize by Feature: Group endpoints by feature or resource. For example, you can have separate files for NFL, NBA, NHL, etc.

- Use Partial Classes: If you prefer to keep everything in one class, you can use partial classes to split the implementation across multiple files.

- Use Route Groups: ASP.NET Core supports route groups, which can help you organize related endpoints together.

 Chose and implemented this option: Organize by Feature: Group endpoints by feature or resource. For example, you can have separate files for NFL, NBA, NHL, etc