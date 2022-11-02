using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Modules
{
    public class PrefixModule : ModuleBase<SocketCommandContext>
    {
        [Command("joserobjr")]
        [Summary("Primeiro comando feito para o bot")]
        public async Task HandlePingCommand()
        {
            await Context.Message.ReplyAsync("https://media.discordapp.net/attachments/896243750228090974/952247379732619326/lobber.gif");
        }

        [Command("fresno")]
        [Summary("Letra da música \"6h34 (NEM LIGA GURIA)\" de Fresno")]
        public async Task HandleFresnoCommand()
        {
            string fresno = "Nem liga, guria\nSe eu já nem sei disfarçar\nSe eu já cansei de esconder\nO que era fácil de achar" +
                "\nNem liga, guria\nSe nos meus olhos não há mais\nO brilho de quem vivia\nCom o coração em paz" +
                "\nSe a gente já soubesse como vai ser a viagem\nAntes mesmo de comprar nossa passagem\nA gente já virava pro outro lado e dormia tão só" +
                "\nSe a gente entendesse que há um ciclo no amor\nComeça pela cura, mas termina com a dor\nA nossa cama pra sempre estaria vazia" +
                "\nNem liga, guria\nSe a minha voz acabar\nSei que tu já me sacou sem eu precisar falar\nNem liga, guria\nNão vou poder te atender" +
                "\nTô encontrando em minha vida um canto só pra você\nSe a gente já soubesse como vai ser a viagem\nNão perderia tanto tempo com bobagem" +
                "\nE o meu peito poderia muito bem ser a tua moradia\nEu finjo que acredito no que dizem sobre o amor\nEu finjo que é eterno, mas te peço, por favor" +
                "\nEsquece tudo e vem passar comigo essa madrugada tão fria\nVê se não fica assustada quando eu digo\nEu nunca fui daqueles que fazem sentido" +
                "\nTô em São Paulo, aqui o céu nunca é azul\nEu tô aqui cantando um samba com sotaque do sul\nE amanheceu, e eu deveria estar dormindo" +
                "\nMas esses versos são palavras explodindo\nE no teu colo, um dia, elas vão cair\nE aonde isso vai dar, só cabe a nós decidir";
            await Context.Message.ReplyAsync(fresno);
        }
    }
}
