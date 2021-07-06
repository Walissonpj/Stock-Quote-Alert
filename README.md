# Stock-Quote-Alert

Aplicação para monitoramento de preço de ações. O objetivo da aplicação é avisar, via e-mail, caso a cotação de um ativo da B3 caia mais do que certo nível, ou suba acima de outro.  

Ele deve ser chamado via linha de comando com 3 parâmetros:  
* O ativo a ser monitorado 
* O preço de referência para venda 
* O preço de referência para compra 

Ex:  
> stock-quote-alert.exe PETR4 22.67 22.59

O arquivo de configuração deve ser preenchido com os campos:
* O e-mail de destino dos alertas
* As configurações de acesso ao servidor de SMTP que irá enviar o e-mail