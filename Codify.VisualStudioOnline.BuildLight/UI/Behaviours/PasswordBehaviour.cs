namespace Codify.VisualStudioOnline.BuildLight.UI.Behaviours
{
    #region Using

    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    #endregion

    /// <summary>
    /// Class PasswordManager
    /// </summary>
    public static class PasswordBehaviour
    {
        #region Constants and Fields

        /// <summary>
        /// The is password control property
        /// </summary>
        public static readonly DependencyProperty IsPasswordControlProperty = DependencyProperty.RegisterAttached("IsPasswordControl", typeof(bool), typeof(PasswordBehaviour), new PropertyMetadata(false, OnIsPasswordControlChanged));

        /// <summary>
        /// The password property
        /// </summary>
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBehaviour), new PropertyMetadata(string.Empty, OnPasswordBindingChanged));

        /// <summary>
        /// The updating password property
        /// </summary>
        private static readonly DependencyProperty UpdatingPasswordProperty = DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBehaviour), new PropertyMetadata(false));

        #endregion

        #region Public Methods

        /// <summary>Gets the is password control.</summary>
        /// <param name="d">The dependency object.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public static bool GetIsPasswordControl(DependencyObject d)
        {
            return (bool)d.GetValue(IsPasswordControlProperty);
        }

        /// <summary>Gets the password.</summary>
        /// <param name="d">The dependency object.</param>
        /// <returns>Returns the current password.</returns>
        public static string GetPassword(DependencyObject d)
        {
            return (string)d.GetValue(PasswordProperty);
        }

        /// <summary>Determines whether the specified d has binding.</summary>
        /// <param name="d">The d.</param>
        /// <returns><c>true</c> if the specified d has binding; otherwise, <c>false</c>.</returns>
        public static bool HasBinding(DependencyObject d)
        {
            return GetPassword(d) != null;
        }

        /// <summary>Sets the is password control.</summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetIsPasswordControl(DependencyObject d, bool value)
        {
            d.SetValue(IsPasswordControlProperty, value);
        }

        /// <summary>Sets the password.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">The value.</param>
        public static void SetPassword(DependencyObject d, string value)
        {
            d.SetValue(PasswordProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>Gets the updating password.</summary>
        /// <param name="d">The d.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private static bool GetUpdatingPassword(DependencyObject d)
        {
            return (bool)d.GetValue(UpdatingPasswordProperty);
        }

        /// <summary>Handles the password box.</summary>
        /// <param name="input">The input.</param>
        /// <param name="bind">if set to <c>true</c> [bind].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private static bool HandlePasswordBox(PasswordBox input, bool bind)
        {
            var handled = false;

            if (input != null)
            {
                if (bind && HasBinding(input))
                {
                    input.PasswordChanged += PasswordBoxChanged;
                }
                else
                {
                    input.PasswordChanged -= PasswordBoxChanged;
                }

                handled = true;
            }

            return handled;
        }

        /// <summary>Inserts the text into the password as the sent offset.</summary>
        /// <param name="password">The password.</param>
        /// <param name="text">The text.</param>
        /// <param name="offset">The offset in the password to insert the new text.</param>
        /// <param name="length">The length.</param>
        /// <returns>The new password</returns>
        private static string InsertText(string password, string text, int offset, int length)
        {
            var newPassword = text;
            if (!string.IsNullOrEmpty(password))
            {
                newPassword = string.Concat(password.Substring(0, offset), text.Substring(offset, length), password.Length > offset ? password.Substring(offset) : string.Empty);
            }

            return newPassword;
        }


        /// <summary>Called when [is password control changed].</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnIsPasswordControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var bind = (e.NewValue != null) && (bool)e.NewValue;

                HandlePasswordBox(d as PasswordBox, bind);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>Called when [password binding changed].</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnPasswordBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var newPassword = (string)e.NewValue;

                var passwordBox = d as PasswordBox;
                if (passwordBox != null)
                {
                    if (!passwordBox.Password.Equals(newPassword))
                    {
                        passwordBox.Password = newPassword;
                        if (string.IsNullOrEmpty(newPassword))
                        {
                            passwordBox.Focus(FocusState.Keyboard);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        /** **********
         * This property is for internal use only. It is used for keeping track of the actual password while the visible password is obscured.
         */

        /// <summary>Passwords the box changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private static void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            var input = sender as PasswordBox;
            if (input != null)
            {
                SetUpdatingPassword(input, true);
                SetPassword(input, input.Password);
                SetUpdatingPassword(input, false);
            }
        }

        /// <summary>Removes text defined by the offset and length parameters from the password.</summary>
        /// <param name="password">The password.</param>
        /// <param name="offset">The offset to start removing text from.</param>
        /// <param name="length">The amount of text to remove.</param>
        /// <returns>The new password.</returns>
        private static string RemoveText(string password, int offset, int length)
        {
            var newPassword = string.Empty;
            if (!string.IsNullOrEmpty(password))
            {
                newPassword = string.Concat(password.Substring(0, offset), password.Length > offset + length ? password.Substring(offset + length) : string.Empty);
            }

            return newPassword;
        }

        /// <summary>Sets the updating password.</summary>
        /// <param name="d">The d.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        private static void SetUpdatingPassword(DependencyObject d, bool value)
        {
            d.SetValue(UpdatingPasswordProperty, value);
        }

        #endregion
    }
}