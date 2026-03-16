Sobre API e HTTP
3.1) Explique a diferença entre os códigos HTTP 200, 201, 204 e 404. Em qual situação cada um é retornado no seu Controller?

200 (OK): É o code padrão pra quando a gente pede algo e o servidor entrega (como por exemplo no GET de listar todos).

201 (Created): Eu usei no POST. Ele avisa que o paciente foi criado com sucesso e até mesmo devolve o link de onde ele está

204 (No Content): Usei no PUT e no DELETE. O servidor avisa que a missão foi cumprida, mas não precisa mandar nenhum dado de volta no corpo da resposta.

404 (Not Found): Quando a pessoa tenta buscar ou deletar um ID que não existe na tabela do Oracle.



3.2) O que o atributo [ApiController] faz? O que acontece se você enviar um JSON com o campo obrigatório vazio?
Esse atributo é tipo um atalho que já faz várias validações sozinho. Se eu mandar um JSON sem o nome do paciente (que é obrigatório), o [ApiController] já barra a requisição de cara e devolve um erro 400 (de Bad Request), sem eu precisar fazer if/else na mão.



3.3) Por que o método GetById retorna NotFound() em vez de retornar null? Qual a diferença para o cliente da API?
Se eu retornar null, a API vai dar um 200 OK com a tela vazia, e quem tiver usando a API (o front por exemplo) vai achar que deu tudo certo mas não veio nada. Retornando NotFound(), eu mando um erro 404. Assim tem como saber exatamente que o erro é "essa pessoa não existe" e pode dai ele tratar isso ali no código dele.



Sobre Entity Framework Core
3.4) O que é o Change Tracker do EF Core? Explique o que acontece internamente quando você chama _ctx.SeuDbSet.Add(objeto) seguido de SaveChangesAsync().
O Change Tracker é tipo um espião interno do EF que fica vigiando o que a gente faz com os objetos. Quando eu dou o .Add(), ele só marca o objeto como "Adicionado". A mágica só acontece no SaveChangesAsync(), onde o EF olha pra esse rastreador, traduz tudo pra SQL real e manda pro banco ja de uma vez só.



3.5) Qual a diferença entre FindAsync(id) e ToListAsync()? Qual SQL cada um gera?

FindAsync(id): É pra buscar um único user pela chave primária. Ele gera um SELECT * FROM TB_PACIENTES WHERE ID = :p0. E tbm se se o objeto já estiver na memória, ele nem vai no banco de novo.

ToListAsync(): Esse traz a tabela inteira. Gera um SELECT * FROM TB_PACIENTES.



3.6) Por que usamos EntityState.Modified no PUT ao invés de buscar o objeto primeiro e alterar campo a campo?
Pq é mais rápido. Se eu buscar primeiro, eu faço um SELECT e depois um UPDATE (duas idas ao banco). Mudando o estado direto pra Modified, eu já falo pro EF gerar o UPDATE direto, economizando o processamento e tempo.



Sobre Mensageria
3.7) Qual a diferença entre comunicação síncrona e assíncrona? Dê um exemplo real (fora do projeto) de cada uma.

A comunicação síncrona acontece quando um sistema envia uma requisição e precisa esperar a resposta para continuar, ou seja, os dois lados precisam estar ativos ao mesmo tempo, como em uma chamada de API HTTP tradicional. Já a comunicação assíncrona ocorre quando o sistema envia a mensagem e não precisa esperar a resposta imediatamente.
Dando um exemplo que a gente sempre usa na vida real pra ficar mais fácil de entender>

Síncrona: É tipo uma ligação de telefone: você fala e fica esperando o outro responder na hora.

Assíncrona: É tipo mandar um áudio no WhatsApp: você manda a mensagem e vai fazer outras coisas, e dai a pessoa responde quando puder.



3.8) O que é o ACK (Acknowledge) no RabbitMQ? O que acontece se o Consumer processar a mensagem mas NÃO enviar o ACK?
O ACK é como se fosse o "visto" da mensagem. É o jeito do Consumer avisar o RabbitMQ: "pode apagar essa, eu já cuidei dela". Se o Consumer não mandar o ACK (como por ex se o programa cair no meio), o RabbitMQ coloca a mensagem de volta na fila pra outro tentar processar, pra não perder o dado.
 


3.9) Por que o RabbitMqConsumer herda de BackgroundService e não de ControllerBase? Qual a diferença de ciclo de vida?
O ControllerBase só nasce e morre quando alguém faz um pedido na API. O BackgroundService é um worrker que nasce quando o projeto liga e fica rodando no fundo o tempo todo, que é o que a gente precisa pra ficar ouvindo a fila do RabbitMQ sem parar.



3.10) Se o RabbitMQ estiver fora do ar no momento do POST, o que acontece? O produto é salvo no Oracle? A API retorna erro? Sugira uma melhoria para tratar esse caso.
Do jeito que tá agora, o paciente salva no Oracle, mas na hora de enviar pro RabbitMQ a API vai travar e dar erro 500 pro usuário. Uma ideia pra melhorar eu acho que seria salvar a mensagem numa nova tabela de "pendentes" no próprio banco e ter um robô que fica tentando reenviar pro RabbitMQ até ele voltar ou usar o Outbox.