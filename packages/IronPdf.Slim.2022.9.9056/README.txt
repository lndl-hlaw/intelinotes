IronPDF is a .NET library to generate, read, edit & save PDF files in .NET projects. IronPDF features HTML to PDF for .NET 6, .NET 5, Core, Standard & Framework with full HTML to PDF support including CSS3 and JS.

Visit our website for a quick-start guide at https://ironpdf.com/docs/

C# Code Example:
========================
using IronPdf;
var Renderer = new ChromePdfRenderer(); // Instantiates Chrome Renderer
var pdf = Renderer.RenderHtmlAsPdf(" <h1> ~Hello World~ </h1> Made with IronPDF!");
pdf.SaveAs("html_saved.pdf"); // Saves our PdfDocument object as a PDF
 
Documentation Links
========================
Code Examples : https://ironpdf.com/examples/
API Reference : https://ironpdf.com/object-reference/api/
Tutorials : https://ironpdf.com/tutorials/
Licensing : https://ironpdf.com/licensing/
Support : developers@ironsoftware.com

Compatibility
========================
* C#, F#, and VB.NET
* .NET 6, .NET 5, Core 2x & 3x, Standard 2, and Framework 4x
* Console, Web, and Desktop Apps
* Windows, macOs, Linux (Debian, CentOS, Ubuntu), Docker, Azure, and AWS
* Microsoft Visual Studio or Jetbrains ReSharper & Rider
