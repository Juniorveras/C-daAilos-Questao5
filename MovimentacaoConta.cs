[HttpPost]
public async Task<IActionResult> MovimentarContaCorrente([FromBody] MovimentarContaCorrenteRequest request)
{
    // Valida parametros
    if (!IsValidRequest(request))
    {
        return BadRequest("Invalid request");
    }

    // Check se o valor existe e se a conta esta ativa
    var contaCorrente = await GetContaCorrenteAsync(request.IdContaCorrente);
    if (contaCorrente == null || !contaCorrente.Ativo)
    {
        return BadRequest("Invalid account or inactive");
    }

    // Check se o valor é positivo
    if (request.Valor <= 0)
    {
        return BadRequest("Invalid value");
    }

    // Check se a movimentação é do Tipo valido
    if (request.TipoMovimento != "C" && request.TipoMovimento != "D")
    {
        return BadRequest("Invalid movement type");
    }

    // Cria nova Movimenttação
    var movimento = new Movimento
    {
        IdContaCorrente = request.IdContaCorrente,
        DataMovimento = DateTime.Now,
        TipoMovimento = request.TipoMovimento,
        Valor = request.Valor
    };

    // Salva DB
    await SaveMovimentoAsync(movimento);

    // Returno ID
    return Ok(movimento.IdMovimento);
}

private async Task<ContaCorrente> GetContaCorrenteAsync(string idContaCorrente)
{
    // Uso do Dapper para consulta
    using (var connection = new SqlConnection(_databaseConnectionString))
    {
        connection.Open();
        var contaCorrente = await connection.QuerySingleOrDefaultAsync<ContaCorrente>("SELECT * FROM contacorrente WHERE idcontacorrente = @idcontacorrente", new { idcontacorrente });
        return contaCorrente;
    }
}

private async Task SaveMovimentoAsync(Movimento movimento)
{
    // Uso do Dapper para salvar a movimentação
    using (var connection = new SqlConnection(_databaseConnectionString))
    {
        connection.Open();
        await connection.ExecuteAsync("INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor) VALUES (@idmovimento, @idcontacorrente, @datamovimento, @tipomovimento, @valor)", movimento);
    }
}

private bool IsValidRequest(MovimentarContaCorrenteRequest request)
{
    // Valida request parametros
    return request != null && !string.IsNullOrEmpty(request.IdContaCorrente) && request.Valor > 0 && (request.TipoMovimento == "C" || request.TipoMovimento == "D");
}

public class MovimentarContaCorrenteRequest
{
    public string IdContaCorrente { get; set; }
    public decimal Valor { get; set; }
    public string TipoMovimento { get; set; }
}