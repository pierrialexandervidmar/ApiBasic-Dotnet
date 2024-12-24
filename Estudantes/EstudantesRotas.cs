namespace ApiBasic.Estudantes;

public static class EstudantesRotas
{
    public static void AddRotasEstudantes(WebApplication app)
    {
        app.MapGet("estudantes", () => "Hello Estudantes");
    }
}