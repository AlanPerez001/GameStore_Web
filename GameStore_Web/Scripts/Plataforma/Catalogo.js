

function eliminaJuego(idElement) {
    var idRegistro = idElement.replace("btnEliminar", "");
    console.log("cargo scrip juego");
    if (idRegistro > 0) {
        console.log($("#IdJuegoEliminar").val(idRegistro));
        $("#IdJuegoEliminar").val(idRegistro);
        //$("#modalConfirmacionElimina").modal();
        $("#formEliminar").submit(); SubmitPagoUsuario
    }
    else {
        $("#modalResultadoTitulo").text("Validación");
        $("#modalResultadoMensaje").text("No fue posible obtener el edificio a eliminar.");
        $("#modalResultado").modal("show");
    }
}