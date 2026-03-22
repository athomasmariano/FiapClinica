using FiapClinica.Data;
using FiapClinica.Messaging;
using FiapClinica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FiapClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PacientesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _context.Pacientes.ToListAsync();
        return Ok(pacientes); // Retorna 200
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);

        if (paciente == null)
            return NotFound(); // Retorna 404

        return Ok(paciente); // Retorna 200
    }

    [HttpPost]
    public async Task<IActionResult> Create(Paciente paciente)
    {
        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        // DESAFIO EXTRA 4.3: Try/Catch no RabbitMQ
        try
        {
            RabbitMqProducer.Publish(paciente);
        }
        catch (Exception ex)
        {
            // Se o RabbitMQ estiver desligado, o código cai aqui, avisa no console, 
            // mas não quebra a API. O paciente continua sendo salvo (Retorna 201).
            Console.WriteLine($"\n [AVISO] Paciente salvo no Oracle, mas erro ao conectar no RabbitMQ: {ex.Message}");
        }

        return CreatedAtAction(nameof(GetById), new { id = paciente.Id }, paciente);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Paciente paciente)
    {
        if (id != paciente.Id)
            return BadRequest();

        _context.Entry(paciente).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent(); // Retorna 204
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);

        if (paciente == null)
            return NotFound(); // Retorna 404

        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();

        return NoContent(); // Retorna 204
    }
}