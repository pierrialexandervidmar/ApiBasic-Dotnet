using ApiBasic.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiBasic.Estudantes;

public static class EstudantesRotas
{
    public static void AddRotasEstudantes(this WebApplication app)
    {
        var rotasEstudantes = app.MapGroup("estudantes");
        
        // POST PARA CRIAR
        rotasEstudantes.MapPost("", async (AddEstudanteRequest request, AppDbContext context) =>
            {
                var jaExiste = await context.Estudantes
                    .AnyAsync(estudante => estudante.Nome == request.Nome);
                
                if (jaExiste)
                    return Results.Conflict("Usuário já existe na base de dados");
                
                try
                {
                    var novoEstudante = new Estudante(request.Nome);
                    await context.Estudantes.AddAsync(novoEstudante);
                    await context.SaveChangesAsync();

                    // Retorna 201 Created com o estudante recém-criado                              
                    return Results.Created($"/estudantes/{novoEstudante.Id}", novoEstudante);
                }
                catch (Exception ex)
                {
                    // Em caso de erro, retorna 400 Bad Request com uma mensagem
                    return Results.BadRequest(new { error = ex.Message });
                }
            })
            .WithName("CriarEstudante")
            .WithTags("Estudantes")
            .WithSummary("Rota responsável por criar um novo estudante.")
            .Produces<Estudante>(StatusCodes.Status201Created) // Indica o tipo de resposta e código HTTP
            .Produces(StatusCodes.Status400BadRequest);
        
        // GET PARA RETORNAR TODOS OS USUÁRIOS
        rotasEstudantes.MapGet("", async (AppDbContext context) =>
            {
                var estudantes = await context
                    .Estudantes
                    .Where(estudante => estudante.Ativo)
                    .ToListAsync();

                return Results.Ok(estudantes);
            })
            .WithTags("Estudantes")
            .WithSummary("Rota responsável por buscar todos os estudantes.")
            .Produces<List<Estudante>>(StatusCodes.Status200OK); // Indica o tipo de resposta e código HTTP
    }
}
