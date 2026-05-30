# Shaos

[![Build](https://github.com/DerekGn/Shaos/actions/workflows/build.yml/badge.svg)](https://github.com/DerekGn/Shaos/actions/workflows/build.yml)

## Domain Model

```mermaid

---
title: Domain Model
---

classDiagram

namespace Shaos.Repository.Models {
    class BaseEntity {
        +DateTime CreatedDate
        +int Id
        +DateTime UpdatedDate
    }
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
    class PlugInChildBase {
        +PlugIn PlugIn
    }
    class PlugInInformation {
        +string AssemblyFileName
        +string AssemblyVersion
        +string Directory
        +bool HasConfiguration
        +bool HasLogger
        +string PackageFileName
        +PlugIn PlugIn
        +string TypeName
    }
    class PlugInInstance {
        +string? Configuration
        +string Description
        +bool Enabled
        +string Name
        +List<Device> Devices
    }
}

namespace Shaos.Repository.Models.Devices {
    class Device {
        +int InstanceId
        +string Name
        +List<BaseParameter> Parameters
        +PlugInInstance? PlugInInstance
    }
}

namespace Shaos.Repository.Models.Devices.Parameters {
    class BaseParameter {
        +bool CanWrite
        +int? DashboardItemId
        +Device? Device
        +int InstanceId
        +string Name
        +ParameterType? ParameterType
        +DateTime TimeStamp
        +string? Units
        +ICollection<DashboardItem> DashboardItems
    }
    class BaseParameterValue {
        +int Id
        +DateTime TimeStamp
    }
    class BoolParameter {
        +bool Value
        +List<BoolParameterValue> Values
    }
    class BoolParameterValue {
        +bool Value
        +BoolParameter Parameter
    }
    class FloatParameter {
        +float Max
        +float Min
        +float Step
        +float Value
        +List<FloatParameterValue> Values
    }
    class FloatParameterValue {
        +float Value
        +FloatParameter Parameter
    }
    class IntParameter {
        +int Max
        +int Min
        +int Step
        +int Value
        +List<IntParameterValue> Values
    }
    class IntParameterValue {
        +int Value
        +IntParameter Parameter
    }
    class StringParameter {
        +string Value
        +List<StringParameterValue> Values
    }
    class StringParameterValue {
        +string Value
        +StringParameter Parameter
    }
    class UIntParameter {
        +uint Max
        +uint Min
        +uint Step
        +uint Value
        +List<UntParameterValue> Values
    }
    class UIntParameterValue {
        +uint Value
        +List<UIntParameterValue> Values
    }
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
    BaseParameterValue <|-- UIntParameterValue

    Device *-- BaseParameter
    Device *-- PlugInInstance

    BaseParameter *-- Device
    BaseParameter *-- DashboardItem

    BoolParameter *-- BoolParameterValue
    FloatParameter *-- FloatParameterValue
    IntParameter *-- IntParameterValue
    StringParameter *-- StringParameterValue
    UIntParameter *-- UIntParameterValue

    DashboardItem o-- BaseParameter

    PlugInInstance *-- Device

    PlugIn *-- PlugInInstance
    PlugIn *-- PlugInInformation

    PlugInInformation *-- PlugIn

    PlugInInstance *-- Device
```
