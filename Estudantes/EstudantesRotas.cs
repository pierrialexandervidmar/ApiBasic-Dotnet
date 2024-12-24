using ApiBasic.Data;

namespace ApiBasic.Estudantes;

public static class EstudantesRotas
{
    public static void AddRotasEstudantes(this WebApplication app)
    {
        var rotasEstudantes = app.MapGroup("estudantes");
        
        // POST PARA CRIAR
        rotasEstudantes.MapPost("", async (AddEstudanteRequest request, AppDbContext context) =>
        {
            var novoEstudante = new Estudante(request.Nome);
            await context.Estudantes.AddAsync(novoEstudante);
            await context.SaveChangesAsync();
        });
    }
}
