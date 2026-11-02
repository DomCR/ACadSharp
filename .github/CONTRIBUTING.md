# Contribute to ACadSharp

## Report bugs

- Before creating a new issue, please ensure the bug was not already reported or there is already a similar one 
by searching on GitHub under [issues](https://github.com/DomCR/ACadSharp/issues).
- Otherwise [open an issue](https://github.com/DomCR/ACadSharp/issues/new?template=bug_report.md) addressing the problem. Please try to follow the template which helps to identify the problem faster.
- If you write a snipped of code, make sure that is [correctly formatted](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-and-highlighting-code-blocks) and specify the language used in it.

## Request a new feature

- Use the following [template](https://github.com/DomCR/ACadSharp/issues/new?template=feature_request.md) to propose a new feature.

## Pull Request Guidelines

Clear, focused pull requests are easier to review and faster to merge. Follow these guidelines when you submit a PR.

### Keep the PR focused

- Each PR should handle **one single issue or feature**.
- Don't mix unrelated changes (refactoring, formatting, renaming, or other fixes) with the main change. Open a separate PR for each one.
- If you find another problem while working, open a new issue for it instead of fixing it in the same PR.
- Keep the diff as small as possible. Large PRs are harder to review and more likely to be delayed.

### Show what the PR fixes or adds

Every PR must include evidence that it fixes the error or that the feature works. Provide **at least one** of the following:

#### Screenshots (before and after)

For visual or output-related changes, include screenshots that show the result **before** and **after** the change, for example the file opened in a CAD viewer.

| Before | After |
|--------|-------|
| ![before](link-to-image) | ![after](link-to-image) |

#### Example files

- Attach the **DWG/DXF file** that reproduces the issue.
- Note the file version (for example `AC1018`, `AC1032`) and the application used to create it.

#### Testable code

Include a minimal code snippet that reproduces the problem, and add or update **unit tests** that cover the fix or feature.
Note: for a roundtrip test, you can use the existing `WriterSingleObjectTests` class which provides a simple way to test reading and writing for all versions.

### AI development

For AI-generated code contributions there are a few more steps to follow before submitting the PR.
**Don't let an AI write a pull request without supervision**. Make a self review before submitting.

- **Understand the change.** Review and understand all AI-generated code before you submit it. You should be able to explain why each change is needed and how it works.
- **Avoid unecessary comments.** AI often adds comments that are not useful or accurate. Remove them if they don't add value.
- **Keep the scope small.** AI tools often change unrelated code: they reformat files, rename things, refactor or add extra "improvements". Remove anything outside the PR's single issue or feature.
- **Check that it's correct.** AI can invent DXF group codes, DWG object structures or spec details that don't exist. Check them against the specification and real sample files.
- **Test it.** Build the solution, run the tests, and confirm the fix with real DWG/DXF files. Code that only compiles isn't enough.
- **Follow the project's style.** Make sure the code matches the existing conventions, architecture and patterns in the repository.
- **Write the description yourself.** PR descriptions should be clear and accurate. Don't paste long, generic AI summaries.
- **Say when you used AI.** If AI generated a large part of the change, mention it in the PR description.

PRs that look like unreviewed AI output may be closed without review.

## Building

Before building run:

```console
git submodule update --init --recursive
```

This command will clone the submodules necessary to build the project.
