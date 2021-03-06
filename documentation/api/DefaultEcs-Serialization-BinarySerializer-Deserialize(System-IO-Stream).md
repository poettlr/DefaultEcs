#### [DefaultEcs](./index.md 'index')
### [DefaultEcs.Serialization](./DefaultEcs-Serialization.md 'DefaultEcs.Serialization').[BinarySerializer](./DefaultEcs-Serialization-BinarySerializer.md 'DefaultEcs.Serialization.BinarySerializer')
## BinarySerializer.Deserialize(System.IO.Stream) Method
Deserializes a [World](./DefaultEcs-World.md 'DefaultEcs.World') instance from the given [System.IO.Stream](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Stream 'System.IO.Stream').  
```csharp
public DefaultEcs.World Deserialize(System.IO.Stream stream);
```
#### Parameters
<a name='DefaultEcs-Serialization-BinarySerializer-Deserialize(System-IO-Stream)-stream'></a>
`stream` [System.IO.Stream](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Stream 'System.IO.Stream')  
The [System.IO.Stream](https://docs.microsoft.com/en-us/dotnet/api/System.IO.Stream 'System.IO.Stream') from which the data will be loaded.  
  
#### Returns
[World](./DefaultEcs-World.md 'DefaultEcs.World')  
The [World](./DefaultEcs-World.md 'DefaultEcs.World') instance loaded.  
#### Exceptions
[System.ArgumentNullException](https://docs.microsoft.com/en-us/dotnet/api/System.ArgumentNullException 'System.ArgumentNullException')  
[stream](#DefaultEcs-Serialization-BinarySerializer-Deserialize(System-IO-Stream)-stream 'DefaultEcs.Serialization.BinarySerializer.Deserialize(System.IO.Stream).stream') is null.  
