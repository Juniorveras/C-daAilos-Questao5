[HttpGet]
public async Task<IActionResult> ConsultarSaldoContaCorrente(string idContaCorrente)
{
    // Check se a conta existe e se esta ativa
    var contaCorrente = await GetContaCorrenteAsync(idContaCorrente);
    if (contaCorrente == null || !contaCorrente.Ativo)
    {
        return BadRequest("Invalid account or inactive");
    }

    // Saldo
    var saldo = await CalculateSaldoAsync(idContaCorrente);

    // Retorno Saldo
    return Ok(new ConsultarSaldoResponse
    {
        NumeroContaCorrente = contaCorrente.Numero,
        NomeTitular = contaCorrente.Nome,
        DataHoraResposta = DateTime.Now,
        Saldo = saldo
    });
}

private async Task<decimal> CalculateSaldoAsync(string idContaCorrente)
{
    // Uso do Dapper consulta DB
    using (var connection = new SqlConnection(_databaseConnectionString))
    {
        connection.Open();
        var creditos = await connection.QueryAsync<decimal>("SELECT SUM(valor) FROM movimento WHERE idcontacorrente = @idcontacorrente AND tipomovimento = 'C'", new { idcontacorrente });
        var debitos = await connection.QueryAsync<decimal>("SELECT SUM(valor) FROM movimento WHERE idcontacorrente = @idcontacorrente AND tipomovimento = 'D'", new { idcontacorrente });
        return creditos.FirstOrDefault() - debitos.FirstOrDefault();
    }
}

public class ConsultarSaldoResponse
{
    public int NumeroContaCorrente { get; set; }
    public string NomeTitular { get; set; }
    public DateTime DataHoraResposta { get; set; }
    public decimal Saldo { get; set; }
}