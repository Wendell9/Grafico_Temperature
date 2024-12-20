﻿using Grafico_Temperature.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TemperatureController : Controller
{
	private readonly TemperatureDAO _temperatureDAO;
	public TemperatureController()
	{
		_temperatureDAO = new TemperatureDAO(); // Inicializa a classe DAO

	}

	// Método para obter os dados da API e passar para a View
	public async Task<IActionResult> Index()
	{
		try
		{
			// Chama o DAO para obter os dados da API
			var temperatureData = await _temperatureDAO.GetTemperatureDataAsync();

			// Preenche o ViewModel diretamente no Controller
			var viewModel = new TemperatureGraphViewModel();
			viewModel.Temperatures = temperatureData.Select(t => t.Temperature).ToList();
			viewModel.Timestamps = temperatureData.Select(t => t.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")).ToList();
			viewModel.AverageTemperature = 0.0;
			viewModel.LastTemperature = 0.0;

			return View(viewModel); // Passa o ViewModel para a View
		}
		catch (Exception ex)
		{
			// Em caso de erro na requisição da API, exibe a mensagem de erro
			ViewBag.ErrorMessage = "Erro ao obter dados: " + ex.Message;
			return View();
		}


	}
	[HttpGet]
	public async Task<IActionResult> GetTemperatureData()
	{
		var timestamps = new List<string> { /* Dados de timestamp */ };
		var temperatures = new List<double> { /* Dados de temperatura */ };

		var temperatureData = await _temperatureDAO.GetTemperatureDataAsync();

		// Preenche o ViewModel diretamente no Controller
		temperatures = temperatureData.Select(t => t.Temperature).ToList();
		timestamps = temperatureData.Select(t => t.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")).ToList();

		return Json(new
		{
			Timestamps = timestamps,
			Temperatures = temperatures
		});
	}

	public async Task<IActionResult> ColetaUltimaTemperatura(double trigger)
	{
		var lastTemperature = await _temperatureDAO.GetLastTemperature();
		VerificaGatilho(Convert.ToDouble(lastTemperature),trigger);
		return Json(new
		{
			Lasttemperature = lastTemperature
		});
	}

	public IActionResult RenderGraficoTemperatura(int setPoint)
	{
		TemperatureGraphViewModel gr = new TemperatureGraphViewModel();
		gr.SetPoint = setPoint;
		// Aqui, você passa o modelo necessário para a partial view
		return PartialView("~/Views/Temperature/pvGraficoTemperatura.cshtml", gr);
	}

	public async void VerificaGatilho(double teste, double TGatilho)
	{
		string status = await _temperatureDAO.GetLEDStatus();

		if (teste > TGatilho)
		{

			if (status == "off")
			{
				await _temperatureDAO.SwitchLed("on");
			}
		}
		else
		{
			if (status == "on")
			{
				await _temperatureDAO.SwitchLed("off");
			}
		}
	}
	public async Task<IActionResult> VerificarEstabilizacao(string temperaturasArray)
	{
		double[] temperaturas = temperaturasArray
			.Split(',')
			.Select(x => Convert.ToDouble(x, System.Globalization.CultureInfo.InvariantCulture))
			.ToArray();

		double tolerancia = 0.5;

		// Calcula a variação máxima na janela
		double variacaoMaxima = temperaturas.Max() - temperaturas.Min();

		// Retorna verdadeiro se a variação máxima for menor ou igual à tolerância
		if (variacaoMaxima <= tolerancia)
		{
			return Json(true);
		}
		else
		{
			return Json(false);
		}
	}

}

