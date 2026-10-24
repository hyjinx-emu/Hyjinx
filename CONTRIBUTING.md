# Contribution Guidelines

You can contribute to Hyjinx with PRs, testing of PRs and issues. Contributing code and other implementations is greatly appreciated alongside simply filing issues for problems you encounter.

Please read the entire document before continuing as it can potentially save everyone involved a significant amount of time.

## Reporting Issues
We always welcome bug reports, feature proposals and overall feedback. Here are a few tips on how you can make reporting your issue as effective as possible:

#### Finding Existing Issues
Before filing a new issue, please search our [open issues](https://github.com/hyjinx-emu/Hyjinx/issues) to check if it already exists.

If you do find an existing issue, please include your own feedback in the discussion. Do consider upvoting (👍 reaction) the original post, as this helps us prioritize popular issues in our backlog.

#### Writing a Good Bug Report
Good bug reports make it easier for maintainers to verify and root cause the underlying problem. The better a bug report, the faster the problem will be resolved. 
Ideally, a bug report should contain the following information:

* A high-level description of the problem.
* A _minimal reproduction_, i.e. the smallest time commitment/configuration required to reproduce the wrong behavior. This can be in the form of a small homebrew application, or by providing a save file and reproduction steps for a specific game.
* A description of the _expected behavior_, contrasted with the _actual behavior_ observed.
* Information on the environment: OS/distro, CPU, GPU (including driver), RAM etc.
* A log file of the run instance where the issue occurred. Log files can be found in `[Executable Folder]/logs` and are named chronologically.
* Additional information, e.g. is it a regression from previous versions? Are there any known workarounds?

When ready to submit a bug report, please use the Bug Report issue template.

## Contributing Changes

Project maintainers will merge changes that both improve the project and meet our standards for code quality.

### DOs and DON'Ts

Please do:

* **DO** follow our [coding style](docs/coding-guidelines/coding-style.md) (C# code-specific).
* **DO** give priority to the current style of the project or file you're changing even if it diverges from the general guidelines.
* **DO** keep the discussions focused. When a new or related topic comes up
  it's often better to create new issue than to side track the discussion.
* **DO** clearly state on an issue that you are going to take on implementing it.
* **DO** blog and tweet (or whatever) about your contributions, frequently!

Please do not:

* **DON'T** make PRs for style changes.
* **DON'T** surprise us with big pull requests, they will be rejected.
* **DON'T** commit code that you didn't write. If asked, be honest whether AI was involved with any code changes as that may require further scrutiny of the change.
* **DON'T** submit PRs that alter licensing related files or headers. If you believe there's a problem with them, file an issue and we'll be happy to discuss it.

Thank you for your willingness to continue to the project, we look forward to seeing your improvements!