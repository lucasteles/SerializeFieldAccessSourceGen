using TestProgram;
using FooNamespace;
using BarNamespace;

var foo = new Foo();
foo.SetSerializeField_cooldown(5);

var bar = new Bar();
bar.SetSerializeField_jump(true);

Console.WriteLine(foo);
Console.WriteLine(bar);
