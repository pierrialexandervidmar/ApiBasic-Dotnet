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

                    var estudanteRetorno = new EstudanteDto(novoEstudante.Id, novoEstudante.Nome);

                    // Retorna 201 Created com o estudante recém-criado                              
                    return Results.Created($"/estudantes/{novoEstudante.Id}", estudanteRetorno);
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
            .Produces<EstudanteDto>(StatusCodes.Status201Created) // Indica o tipo de resposta e código HTTP
            .Produces(StatusCodes.Status400BadRequest);
        
        // GET PARA RETORNAR TODOS OS USUÁRIOS
        rotasEstudantes.MapGet("", async (AppDbContext context) =>
            {
                var estudantes = await context
                    .Estudantes
                    .Where(estudante => estudante.Ativo)
                    .Select(estudante => new EstudanteDto(estudante.Id, estudante.Nome))
                    .ToListAsync();

                return Results.Ok(estudantes);
            })
            .WithTags("Estudantes")
            .WithSummary("Rota responsável por buscar todos os estudantes.")
            .Produces<List<EstudanteDto>>(StatusCodes.Status200OK); // Indica o tipo de resposta e código HTTP
        
        // PUT PARA O UPDATE DE ESTUDANTE
        rotasEstudantes.MapPut("{id:guid}", async (Guid id, UpdateEstudanteRequest request, AppDbContext context) =>
            {
                var estudante = await context.Estudantes
                    .SingleOrDefaultAsync(estudante => estudante.Id == id);

                if (estudante == null)
                    return Results.NotFound(new { Message = "Estudante não encontrado." });

                // Atualizar nome se fornecido
                if (!string.IsNullOrEmpty(request.Nome))
                {
                    estudante.AtualizarNome(request.Nome);
                }

                // Atualizar o status se fornecido
                if (request.Ativo.HasValue)
                {
                    estudante.AtualizarAtivo(request.Ativo.Value);
                }

                await context.SaveChangesAsync();

                // Retornar o recurso atualizado
                return Results.Ok(estudante);
            })
            .WithTags("Estudantes")
            .WithSummary("Rota responsável por atualizar o nome e/ou status de ativo de um estudante.")
            .Produces<Estudante>(StatusCodes.Status200OK) // Sucesso com o estudante atualizado
            .Produces(StatusCodes.Status404NotFound);

        rotasEstudantes.MapDelete("{id:guid}",
                async (Guid id, AppDbContext context) =>
                {
                    var estudante = await context.Estudantes
                        .SingleOrDefaultAsync(estudante => estudante.Id == id);

                    if (estudante == null)
                        return Results.NotFound(new { Message = "Estudante não encontrado." });

                    // Atualizar o status se fornecida
                    estudante.AtualizarAtivo(false);

                    await context.SaveChangesAsync();

                    // Retornar o recurso atualizado
                    return Results.NoContent();
                })
            .WithName("DesativarEstudante")
            .WithTags("Estudantes")
            .WithSummary("Rota responsável por inativar um estudante.")
            .Produces(StatusCodes.Status204NoContent); // Indica o tipo de resposta e código HTTP
    }
}
