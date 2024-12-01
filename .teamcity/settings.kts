import jetbrains.buildServer.configs.kotlin.*

/*
The settings script is an entry point for defining a TeamCity
project hierarchy. The script should contain a single call to the
project() function with a Project instance or an init function as
an argument.

VcsRoots, BuildTypes, Templates, and subprojects can be
registered inside the project using the vcsRoot(), buildType(),
template(), and subProject() methods respectively.

To debug settings scripts in command-line, run the

    mvnDebug org.jetbrains.teamcity:teamcity-configs-maven-plugin:generate

command and attach your debugger to the port 8000.

To debug in IntelliJ Idea, open the 'Maven Projects' tool window (View
-> Tool Windows -> Maven Projects), find the generate task node
(Plugins -> teamcity-configs -> teamcity-configs:generate), the
'Debug' option is available in the context menu for the task.
*/

version = "2024.03"

project {
    description = "Versioning framework used by other Git2SemVer projects"

    buildType(DeployLocalTeamCityPackage)
    buildType(BuildAndTest)

    features {
    }

    cleanup {
        baseRule {
            artifacts(builds = 20, days = 5)
        }
    }
}

object BuildAndTest : BuildType({
    name = "Build and test"

    artifactRules = """
        +:Git2SemVer.Framework/nupkg/NoeticTools.*.nupkg
        +:SolutionVersioningProject/obj/Git2SemVer.MSBuild.log
        +:SolutionVersioningProject/.git2semver/Git2SemVer.VersionInfo.g.json
    """.trimIndent()

    params {
        param("BuildConfiguration", "Release")
    }

    vcs {
        root(DslContext.settingsRoot)

        cleanCheckout = true
    }

    steps {
        script {
            name = "Clear NuGet caches"
            id = "Clear_NuGet_caches"
            enabled = false
            scriptContent = "dotnet nuget locals all --clear"
        }
        dotnetRestore {
            name = "Restore"
            id = "Restore"
            sources = """
                https://api.nuget.org/v3/index.json
                http://10.1.10.78:8111/guestAuth/app/nuget/feed/_Root/TeamCity/v3/index.json
            """.trimIndent()
        }
        dotnetBuild {
            name = "Build"
            id = "dotnet"
            configuration = "%BuildConfiguration%"
            args = "-p:Git2SemVer_BuildNumber=%build.number% --verbosity normal"
        }
        dotnetTest {
            name = "Test"
            id = "dotnet_1"
            configuration = "%BuildConfiguration%"
            skipBuild = true
            param("dotNetCoverage.dotCover.filters", """
                +:NoeticTools.*
                -:NoeticTools.*Tests
            """.trimIndent())
        }
    }

    triggers {
        vcs {
        }
    }

    failureConditions {
        executionTimeoutMin = 3
        failOnMetricChange {
            enabled = false
            metric = BuildFailureOnMetric.MetricType.TEST_COUNT
            threshold = 20
            units = BuildFailureOnMetric.MetricUnit.PERCENTS
            comparison = BuildFailureOnMetric.MetricComparison.LESS
            compareTo = build {
                buildRule = lastSuccessful()
            }
        }
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.ARTIFACT_SIZE
            threshold = 25
            units = BuildFailureOnMetric.MetricUnit.PERCENTS
            comparison = BuildFailureOnMetric.MetricComparison.LESS
            compareTo = build {
                buildRule = lastSuccessful()
            }
        }
    }

    features {
        perfmon {
        }
    }

    requirements {
        exists("DotNetCLI_Path")
    }
})

object DeployLocalTeamCityPackage : BuildType({
    name = "Deploy (local TeamCity) - package"
    description = "Deploy NuGet package"

    enablePersonalBuilds = false
    type = BuildTypeSettings.Type.DEPLOYMENT
    buildNumberPattern = "%build.counter% (${BuildAndTest.depParamRefs.buildNumber})"
    maxRunningBuilds = 1

    steps {
        dotnetNugetPush {
            name = "Push NuGet package"
            id = "Publish2"
            packages = "NoeticTools.*.nupkg"
            serverUrl = "http://10.1.10.78:8111/httpAuth/app/nuget/feed/_Root/TeamCity/v3/index.json"
            apiKey = "credentialsJSON:bd18b974-1188-423d-9efd-8836806c3669"
        }
    }

    features {
        approval {
            approvalRules = "user:robert"
            manualRunsApproved = false
        }
    }

    dependencies {
        artifacts(BuildAndTest) {
            buildRule = lastSuccessful("""
                +:<default>
                +:*
            """.trimIndent())
            cleanDestination = true
            artifactRules = "+:NoeticTools.Git2SemVer.Framework.*.nupkg"
        }
    }
})

