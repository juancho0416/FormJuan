///pag (Twofive) selecciona todas las casillas
// si estan asociadas
document.addEventListener("DOMContentLoaded",function(){
const areaSelect = document.querySelector('select[name="Input.Area"]');
const empresaSelect = document.querySelector('select[name="Input.Empresa"]');
const isoSelect = document.querySelector('select[name="Input.ISO"]');
const nomSelect = document.querySelector('select[name="Input.NOM"]');
const permisoSelect = document.querySelector('select[name="Input.Permiso"]');
const contratoSelect = document.querySelector('select[name="Input.Contrato"]');
const solicitudSelect = document.querySelector('select[name="Input.Solicitud"]');
const requerimientoSelect = document.querySelector('select[name="Input.Requerimiento"]');
const peticionSelect = document.querySelector('select[name="Input.Peticion"]');

const data = {

"Petroliferos":{
    empresa:"Pemex",
    iso:"ISO-90001",
    nom:"NOM-2390-013",
    permiso:"CNE-P1234-333",
    contrato:"CNE-8905-2345",
    solicitud:"Solicitud 156CNE",
    requerimiento:"Requerimiento 1",
    peticion:"Peticion 299737"
},
"Electricidad":{

    empresa:"CFE",
    iso:"ISO-90002",
    nom:"NOM-3452-093",
    permiso:"CNE-12344-999",
    contrato:"CNE-2839-2929",
    solicitud:"Solicitud 287CNE",
    requerimiento:"Requerimiento 2",
    peticion:"Peticion 334578"
},
"Gas Licuado":{
    empresa:"Privada 1",
    iso:"ISO-90003",
    nom:"NOM-3466-666",
    permiso:"CNE-23399-666",
    contrato:"CNE-2929-2929",
    solicitud:"Solicitud 395CNE",
    requerimiento:"Requerimiento 3",
    peticion:"Peticion 383763"
},
"Gas Natural":{
    empresa:"Privada 2",
    iso:"ISO-90004",
    nom:"NOM-8903-123",
    permiso:"CNE-36779-927",
    contrato:"CNE-2929-2202",
    solicitud:"Solicitud 389CNE",
    requerimiento:"Requerimiento 4",
    peticion:"Peticion 837633"
},
"Hidrocarburos":{
    empresa:"Privada 3",
    iso:"ISO-90005",
    nom:"NOM-7890-234",
    permiso:"CNE-92776-983",
    contrato:"CNE-2812-1234",
    solicitud:"Solicitud 345CNE",
    requerimiento:"Requerimiento 5",
    peticion:"Peticion 235753"
}
};
areaSelect.addEventListener('change',function(){
const area = this.value;
empresaSelect.value="";
isoSelect.value="";
nomSelect.value="";
permisoSelect.value="",
contratoSelect.value="";
solicitudSelect.value="";
requerimientoSelect.value="";
peticionSelect.value="";

if(data[area]){
empresaSelect.value=data[area].empresa;
isoSelect.value=data[area].iso;
nomSelect.value=data[area].nom;
permisoSelect.value=data[area].permiso;
contratoSelect.value=data[area].contrato;
solicitudSelect.value=data[area].solicitud;
requerimientoSelect.value=data[area].requerimiento;
peticionSelect.value=data[area].peticion;
}
});
});
        //selecciona todas las casillas en base a la primera casilla que es el area
document.addEventListener("DOMContentLoaded",function(){
const seccionSelect = document.querySelector('select[name="Input.Seccion"]');
const regulacionSelect = document.querySelector('select[name="Input.Regulacion"]');
const leySelect = document.querySelector('select[name="Input.Ley"]');
const articuloSelect = document.querySelector('select[name="Input.Articulo"]');
const parrafoSelect = document.querySelector('select[name="Input.Parrafo"]');

const data = {
"Electricidad":{
    regulacion:"Regulacion electrica",
    ley:"Ley energia 25-ENG-05",
    articulo:"Articulo XV",
    parrafo:"Parrafo 14"
},
"Gas":{
    regulacion:"Regulacion de gas",
    ley:"Ley gas 25-GS-06",
    articulo:"Articulo XX",
    parrafo:"Parrafo 5"
},
"Petroliferos":{
    regulacion:"Regulacion de petroleo",
    ley:"Ley petroleo 25-PTRL-05",
    articulo:"Articulo XVII",
    parrafo:"Parrafo 3"
},
"Hidrocarburos":{
    regulacion:"Regulacion de hidrocarburos",
    ley:"Ley hidrocarburos 25-HIC-03",
    articulo:"Articulo IX",
    parrafo:"Parrafo 8"
},
"Quimica":{
    regulacion:"Regulacion quimica",
    ley:"Ley quimica 25-QM-02",
    articulo:"Articulo IX",
    parrafo:"Parrafo 7"
}

};
seccionSelect.addEventListener('change',function(){
const seccion = this.value;
regulacionSelect.value="";
leySelect.value="";
articuloSelect.value="";
parrafoSelect.value="";

if(data[seccion]){}
regulacionSelect.value=data[seccion].regulacion;
leySelect.value=data[seccion].ley;
articuloSelect.value=data[seccion].articulo;
parrafoSelect.value=data[seccion].parrafo;

});
});
   ///seleccion de fecha pag(ZeroSecond)
 $gmx(document).ready(function () {
            $('#calendarYear').datepicker({ changeYear: true });
        });


       