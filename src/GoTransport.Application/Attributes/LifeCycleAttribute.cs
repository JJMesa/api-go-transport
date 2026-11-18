namespace GoTransport.Application.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ScopedAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Class)]
public class SingletonAttribute : Attribute
{ }

[AttributeUsage(AttributeTargets.Class)]
public class TransientAttribute : Attribute
{ }