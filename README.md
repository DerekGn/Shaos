# Shaos

[![Build](https://github.com/DerekGn/Shaos/actions/workflows/build.yml/badge.svg)](https://github.com/DerekGn/Shaos/actions/workflows/build.yml)

## Domain Model

```mermaid

---
title: Domain Model
---

classDiagram

namespace Shaos.Repository.Models {
    class BaseEntity
    class DashboardItem {
        +string Label
        +BaseParameter Parameter
    }
    class LogLevelSwitch {
        +string Name
    }
    class PlugIn {
        +string Description
        +List<PlugInInstance> Instances
        +string Name
        +PlugInInformation? PlugInInformation
    }
    class PlugInChildBase
    class PlugInInformation
    class PlugInInstance 
}

namespace Shaos.Repository.Models.Devices {
    class Device {
    }
}

namespace Shaos.Repository.Models.Devices.Parameters {
    class BaseParameter
    class BaseParameterValue
    class BoolParameter
    class BoolParameterValue
    class FloatParameter
    class FloatParameterValue
    class IntParameter
    class IntParameterValue
    class StringParameter
    class StringParameterValue
    class UIntParameter
    class UIntParameterValue
}

    BaseEntity <|-- Device
    BaseEntity <|-- DashboardItem
    BaseEntity <|-- LogLevelSwitch
    BaseEntity <|-- PlugIn
    BaseEntity <|-- PlugInChildBase
    BaseEntity <|-- PlugInInformation

    PlugInChildBase <|-- PlugInInstance

    BaseEntity <|-- BaseParameter

    BaseParameter <|-- BoolParameter
    BaseParameter <|-- FloatParameter
    BaseParameter <|-- IntParameter
    BaseParameter <|-- StringParameter
    BaseParameter <|-- UIntParameter

    BaseParameterValue <|-- BoolParameterValue
    BaseParameterValue <|-- FloatParameterValue
    BaseParameterValue <|-- IntParameterValue
    BaseParameterValue <|-- StringParameterValue
    BaseParameterValue <|-- UIntParameterValue`


```
