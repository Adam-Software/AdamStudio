using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;

namespace AdamStudio.Core.Mvvm
{
    public class ViewModelBase : BindableBase, IDestructible
    {
        protected ViewModelBase() {}

        public virtual void Destroy() {}
    }
}
