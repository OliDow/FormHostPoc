using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace FormHostPoc.Helpers
{
    public abstract class BaseNotify : INotifyPropertyChanged
    {
        #region Methods
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> expression)
        {
            if (expression.NodeType != ExpressionType.Lambda)
                throw new ArgumentException(@"Value must be a lamda expression", "expression");

            var body = expression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("value should be a member expression");

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(body.Member.Name));
            }
        }

        #endregion

        #region Set Property Methods
        protected virtual void Set<TProperty>(Expression<Func<TProperty>> propertyExpression, ref TProperty field, TProperty newValue)
        {
            if (EqualityComparer<TProperty>.Default.Equals(field, newValue))
                return;

            field = newValue;
            OnPropertyChanged(propertyExpression);
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
