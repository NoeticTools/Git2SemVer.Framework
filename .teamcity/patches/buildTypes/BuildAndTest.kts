package patches.buildTypes

import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.failureConditions.BuildFailureOnMetric
import jetbrains.buildServer.configs.kotlin.failureConditions.failOnMetricChange
import jetbrains.buildServer.configs.kotlin.ui.*

/*
This patch script was generated by TeamCity on settings change in UI.
To apply the patch, change the buildType with id = 'BuildAndTest'
accordingly, and delete the patch script.
*/
changeBuildType(RelativeId("BuildAndTest")) {
    failureConditions {
        val feature1 = find<BuildFailureOnMetric> {
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
        feature1.apply {
            enabled = false
        }
    }
}
