using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// Управление общими сведениями о сборке осуществляется с помощью 
// набора атрибутов. Измените значения этих атрибутов, чтобы изменить сведения,
// связанные со сборкой.
[assembly: AssemblyTitle("Simultaneous Text Edit")]
[assembly: AssemblyDescription("Simultaneous Text Edit")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Simultaneous Text Edit")]
[assembly: AssemblyCopyright(@"Copyright © 2016 Peter Carleton
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Параметр ComVisible со значением FALSE делает типы в сборке невидимыми 
// для COM-компонентов.  Если требуется обратиться к типу в этой сборке через 
// COM, задайте атрибуту ComVisible значение TRUE для этого типа.
[assembly: ComVisible(false)]

//Чтобы начать сборку локализованных приложений, задайте 
//<UICulture>CultureYouAreCodingWith</UICulture> в файле .csproj
//внутри <PropertyGroup>.  Например, если используется английский США
//в своих исходных файлах установите <UICulture> в en-US.  Затем отмените преобразование в комментарий
//атрибута NeutralResourceLanguage ниже.  Обновите "en-US" в
//строка внизу для обеспечения соответствия настройки UICulture в файле проекта.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //где расположены словари ресурсов по конкретным тематикам
                                     //(используется, если ресурс не найден на странице 
                                     // или в словарях ресурсов приложения)
    ResourceDictionaryLocation.SourceAssembly //где расположен словарь универсальных ресурсов
                                              //(используется, если ресурс не найден на странице, 
                                              // в приложении или в каких-либо словарях ресурсов для конкретной темы)
)]


// Сведения о версии сборки состоят из следующих четырех значений:
//
//      Основной номер версии
//      Дополнительный номер версии 
//   Номер сборки
//      Редакция
//
// Можно задать все значения или принять номера сборки и редакции по умолчанию 
// используя "*", как показано ниже:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.5.2")]
[assembly: AssemblyFileVersion("1.0.5.2")]
