using TestProgram;

namespace  FooNamespace
{
    public partial class Foo
    {
        [SerializeField] float cooldown = 10;

        public override string ToString() => cooldown.ToString();
    }
}
namespace  BarNamespace
{
    public partial class Bar
    {
        [SerializeField] bool jump = false;

        public override string ToString() => $"Jump: {jump}";
    }
}
