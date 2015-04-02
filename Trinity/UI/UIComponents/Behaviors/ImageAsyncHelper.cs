﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Trinity.UIComponents
{
    internal class ImageAsyncHelper : DependencyObject
    {
        public static readonly DependencyProperty SourceUriProperty = DependencyProperty.RegisterAttached("SourceUri", typeof (Uri), typeof (ImageAsyncHelper), new PropertyMetadata
        {
            PropertyChangedCallback = (obj, e) => ((Image) obj).SetBinding(Image.SourceProperty,

                new Binding("VerifiedUri")
                {
                    Source = new ImageAsyncHelper {GivenUri = (Uri) e.NewValue},
                    IsAsync = true
                })
        });

        private Uri GivenUri;

        public Uri VerifiedUri
        {
            get
            {
                try
                {
                    Dns.GetHostEntry(GivenUri.DnsSafeHost);
                    return GivenUri;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static Uri GetSourceUri(DependencyObject obj)
        {
            return (Uri) obj.GetValue(SourceUriProperty);
        }

        public static void SetSourceUri(DependencyObject obj, Uri value)
        {
            obj.SetValue(SourceUriProperty, value);
        }
    }
}