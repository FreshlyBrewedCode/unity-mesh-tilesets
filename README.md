## upm-template
A template project for making UPM packages for Unity

## How to use

First use this repository as the base of your UPM project repository by forking it.

### __Project Structure__ 
Let's say the name of your package directory is `ABC`. Your UPM project directory will usually be `Packages/ABC` or `Assets/ABC` (let's assume it's `Packages/ABC` for this guide). A sample directory is included in the repo.

Inside which you'd have your `package.json`, `CHANGELOG.md`, `LICENSE`, `README.md`.

To know more about `package.json` (called the UPM Manifest) go [here](https://docs.unity3d.com/Manual/upm-manifestPkg.html)

### __Configure Github Actions (only works when hosting on github.com)__
Go to `.github/workflows/ci.yml` and change the value of `XYZ` to `Packages/ABC`. 

Every time you commit to `master`, this YML file is used to release your UPM package and automatically use a separate upm branch for releases as well as autogenerate tag for release. If you see the upm branch commits, you'll notice that your project directory is the root instead of the Unity project directory. Github Actions is used to automate this bit.

### __Configure Semantic Release__
Semantic releases is used to automate changelog updates as well as incrementing the version field inside `package.json` (the UPM Manifest). 

Go to `.release.json` do make the following changes:

* `XYZ1` to `Packages/ABC`
* `XYZ2` to `Packages/ABC/package.json`
* `XYZ3` to `Packages/ABC/CHANGELOG.md`

When using `git commit`, follow the [Angular Standard](https://github.com/angular/angular/blob/master/CONTRIBUTING.md#commit)

### __Publishing to package registries__
Use `npm publish` inside `Packages/ABC` or check out [OpenUPM](https://openupm.com/docs/#how-it-works)  

## Further Resources
[Favo Yang's Medium Articles](https://medium.com/@favoyang)  
[OpenUPM Docs](https://openupm.com/docs/)  
[Having a Docker Container As Your Private NPM Registry — The Easy Way by Sibeesh Venu](https://medium.com/better-programming/having-a-docker-container-as-your-private-npm-registry-the-easy-way-68159fa94cc4)  

## Contact
[@github](https://www.github.com/adrenak)  
[@www](http://www.vatsalambastha.com)
