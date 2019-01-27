using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Aesalon
{
    public abstract class BindableObject : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            RaisePropertyChangedExplicit(propertyName);
        }

        protected void RaisePropertyChanged<TProperty>(Expression<Func<TProperty>> projection)
        {
            var memberExpression = (MemberExpression)projection.Body;
            RaisePropertyChangedExplicit(memberExpression.Member.Name);
        }

        void RaisePropertyChangedExplicit(string propertyName)
        {
            VerifyProperty(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentException("propertyName must not be null or empty.");

                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //protected void RaisePropertyChanged(string propertyName)
        //{
        //    VerifyProperty(propertyName);

        //    var handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        if (string.IsNullOrEmpty(propertyName))
        //            throw new ArgumentException("propertyName must not be null or empty.");

        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}

        [Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            var type = GetType();
            var propInfo = type.GetProperty(propertyName);
            if (propInfo == null)
            {
                var msg = string.Format("{0} is not a public property of {1}", propertyName, type.FullName);
                Debug.Fail(msg);
            }
        }
    }
}