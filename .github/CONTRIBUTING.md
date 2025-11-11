# Contribute to ACadSharp

## Report bugs

- Before creating a new issue, please ensure the bug was not already reported or there is already a similar one 
by searching on GitHub under [issues](https://github.com/DomCR/ACadSharp/issues).
- Otherwise [open an issue](https://github.com/DomCR/ACadSharp/issues/new?template=bug_report.md) addressing the problem. Please try to follow the template which helps to identify the problem faster.
- If you write a snipped of code, make sure that is [correctly formatted](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-and-highlighting-code-blocks) and specify the language used in it.

## Request a new feature

- Use the following [template](https://github.com/DomCR/ACadSharp/issues/new?template=feature_request.md) to propose a new feature.

## Create a Pull Request

- If you fixed an issue or have developed a useful feature that you would like to include in the library, feel free to create a pull request, I'll give feedback as soon as I can.
- As a guideline you can check the Dxf documentation [here](https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-235B22E0-A567-4CF6-92D3-38A2306D73F3) or the Open Design document in the [reference](https://github.com/DomCR/ACadSharp/tree/master/reference) folder for Dwg. 

## Building

Before building run:

```console
git submodule update --init --recursive
```

This command will clone the submodules necessary to build the project.
