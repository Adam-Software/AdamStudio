using MahApps.Metro.Controls;
using System.Collections.Specialized;
using System.Windows;

namespace AdamStudio.Controls.CustomControls.RegionAdapters
{
    public class FlyoutsControlRegionAdapter : RegionAdapterBase<FlyoutsControl>
    {
        public FlyoutsControlRegionAdapter(IRegionBehaviorFactory factory) : base(factory) { }

        protected override void Adapt(IRegion region, FlyoutsControl regionTarget)
        {
            region.ActiveViews.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (FrameworkElement element in e?.NewItems)
                    {
                        Flyout flyout = new()
                        {
                            Content = element,
                            DataContext = element.DataContext
                        };

                        regionTarget.Items.Add(flyout);
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (FrameworkElement element in e?.OldItems)
                    {
                        regionTarget.Items.Remove(element);
                    }
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
