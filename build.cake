var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

/*******************************************************************
 * TASKS
 *******************************************************************/

Task("Clean").WithCriteria(c => HasArgument("rebuild")).Does(() =>
{
  CleanDirectory("./UltimateStorageSystem/UltimateStorageSystem/bin/");
  CleanDirectory("./UltimateStorageSystem/UltimateStorageSystem/obj/");
});

Task("Build").IsDependentOn("Clean").Does(() =>
{
  DotNetBuild("./", new DotNetBuildSettings
  {
    Configuration = configuration,
  });
});

Task("Package").IsDependentOn("Build").Does(() => {

})

/*******************************************************************
 * EXECUTION
 *******************************************************************/

RunTarget(target);
