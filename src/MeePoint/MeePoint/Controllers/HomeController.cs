using MeePoint.Filters;
using MeePoint.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MeePoint.Interfaces;
using MeePoint.ViewModels;
using MeePoint.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Hosting;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas.Draw;
using Document = iText.Layout.Document;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Xobject;
using iText.Layout.Borders;

namespace MeePoint.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IHostEnvironment _he;

		private readonly ApplicationDbContext _context;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager, IHostEnvironment host)
		{
			_logger = logger;
			_context = context;
			_userManager = userManager;
			_he = host;
		}

		[Authorize]
		public ActionResult MainPage()
		{
			//// Obtém o utilizador que está autenticado
			//IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			//string email = applicationUser?.Email; // will give the user's Email
			//var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			//var entity = _context.Groups.FirstOrDefault(m => m.Name.ToLower() == "main")
			//MainPageViewModel mainPageViewModel = new MainPageViewModel()
			//{
			//	AmountGroups = _context.GroupMembers.Where(m => m.UserID == user.RegisteredUserID).Count(),
			//	//AmountMeetingsHeldLastMonth = _context.Meetings.Where(m=> m.GroupID).Where(m => m.MeetingDate >= DateTime.Now.AddDays(-30))
			//	EntityName = _context.Entities.FirstOrDefault(m => m.EntityID == user.Groups)
			//}

			return View();
		}

		public async Task<ActionResult> GetCalendarData()
		{
			// Obtém o utilizador que está autenticado
			IdentityUser applicationUser = await _userManager.GetUserAsync(User);
			string email = applicationUser?.Email; // will give the user's Email
			var user = _context.RegisteredUsers.FirstOrDefault(m => m.Email == email);

			// Queremos obter a lista de reuniões agendadas para o específico user
			Dictionary<int, ICollection<Meeting>> meetings = _context.GroupMembers
				.Include(m => m.Group)
				.ThenInclude(m => m.Meetings)
				.ThenInclude(m => m.Group)
				.Where(m => m.UserID == user.RegisteredUserID)
				.Select(x => new KeyValuePair<int, ICollection<Meeting>>(x.GroupID, x.Group.Meetings))
				.ToDictionary(x => x.Key, x => x.Value);

			// Inicializar lista de reuniões a enviar para a View
			List<CalendarEvent> events = new List<CalendarEvent>();
			var random = new Random();

			try
			{
				foreach (KeyValuePair<int, ICollection<Meeting>> keyPair in meetings)
				{
					// Para cada grupo queremos enviar cores diferentes, de forma a poder distinguir as reuniões de acordo com o grupo
					var color = String.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9"

					foreach (Meeting meeting in keyPair.Value)
					{
						events.Add(new CalendarEvent()
						{
							title = meeting.Group.Name,
							description = meeting.Name,
							id = meeting.MeetingID,
							backgroundColor = color,
							borderColor = color,
							allDay = "false",
							start = meeting.MeetingDate.ToString("yyyy-MM-ddTHH:mm:ss"),
							end = (meeting.MeetingDate.AddMinutes(meeting.ExpectedDuration)).ToString("yyyy-MM-ddTHH:mm:ss")
						});
					}
				}
			}
			catch (Exception ex)
			{
				return Json(new List<CalendarEvent>());
			}

			return Json(events);
		}

		public IActionResult MeePoint()
		{
			GeneratePDF();

			return View();
		}

		protected void GeneratePDF()
		{
			string entityName = "Universidade Aveiro";
			int meetingID = 400;

			// Obter o caminho onde vai ser armazenado o ficheiro
			string targetPath = Path.Combine(_he.ContentRootPath, "wwwroot/", entityName, "Meetings/", meetingID + "/");
			string directory = Path.GetDirectoryName(targetPath);

			// Criar Ficheiro
			FileInfo file = new FileInfo(targetPath + "ata.pdf");
			if (!file.Directory.Exists) file.Directory.Create();

			// Must have write permissions to the path folder
			PdfWriter writer = new PdfWriter(targetPath + "ata.pdf");
			PdfDocument pdf = new PdfDocument(writer);
			iText.Layout.Document document = new iText.Layout.Document(pdf);
			Paragraph header = new Paragraph("Atas de Reunião")
			   .SetTextAlignment(TextAlignment.CENTER)
			   .SetFontSize(20);

			document.Add(header);

			Paragraph subheader = new Paragraph(entityName)
			   .SetTextAlignment(TextAlignment.CENTER)
			   .SetFontSize(15);

			document.Add(subheader);

			// Line separator
			LineSeparator ls = new LineSeparator(new SolidLine());
			document.Add(ls);

			AddTitle(document, "Abertura");
			AddText(document, $"A reunião ordinária da {entityName}, devidamente convocada e realizada em {DateTime.Now.ToString("d MMM")}, começando às {DateTime.Now.ToString("HH:mm")}. ");
			AddTitle(document, "Estiveram Presentes");

			string[] participants = new string[] { "Manuel", "João", "Maria" };

			foreach (var participant in participants)
			{
				AddParticipant(document, participant);
			}

			AddTitle(document, "Assuntos Discutidos");

			AddText(document, $"Descrição do Assunto da Reunião");

			AddSignature(document, "Nome do Responsável 1", "Nome de um Co-Responsável");
		}

		private static void AddTitle(iText.Layout.Document document, string titleName)
		{
			PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

			Paragraph paragraph1 = new Paragraph(titleName)
				.SetTextAlignment(TextAlignment.LEFT)
				.SetFont(font)
				.SetFontSize(14);
			document.Add(paragraph1);
		}

		private static void AddText(iText.Layout.Document document, string text)
		{
			PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);

			Paragraph paragraph1 = new Paragraph(text)
				.SetTextAlignment(TextAlignment.JUSTIFIED)
				.SetFont(font)
				.SetFontSize(12); ;

			document.Add(paragraph1);
		}

		private static void AddParticipant(iText.Layout.Document document, string text)
		{
			PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ITALIC);

			Paragraph paragraph1 = new Paragraph(text)
				.SetTextAlignment(TextAlignment.LEFT)
				.SetFont(font)
				.SetFontSize(12); ;

			document.Add(paragraph1);
		}

		private static void AddSignature(iText.Layout.Document document, string manager, string coManager)
		{
			PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);

			Table table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth().SetMarginTop(40);

			// Adiciona o espaço para cada um dos responsáveis poder assinar depois de imprimir
			Paragraph signatureManager = new Paragraph(manager).SetFont(font)
				.SetFontSize(11);

			Paragraph signatureCoManager = new Paragraph(coManager).SetFont(font)
				.SetFontSize(11);

			LineSeparator dottedline = new LineSeparator(new DottedLine()).SetMaxWidth(200).SetMarginTop(10);

			AddCell(table, signatureManager);
			AddCell(table, signatureCoManager);

			AddCell(table, dottedline);
			AddCell(table, dottedline);

			document.Add(table);
			document.Close();

			static void AddCell(Table table, IBlockElement signature)
			{
				Cell cell = new Cell();
				cell.Add(signature);
				cell.SetBorder(Border.NO_BORDER);
				table.AddCell(cell);
			}
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}