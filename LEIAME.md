# Documentação de Anotações

Este documento explica as anotações disponíveis para uso em scripts e outras situações em que é
necessário indicar dependências entre objetos ou ações.

## Anotações

As seguintes anotações estão disponíveis:

### @descricao

A anotação `@descricao` é usada para fornecer uma breve descrição do objeto ou ação em única linha.
Essa descrição deve ser clara e concisa, e pode incluir informações sobre a finalidade, os
parâmetros ou outras informações relevantes.

Exemplo de uso:

    -- @descricao Este script atualiza a tabela de vendas com dados recentes de vendas.

### @requerido-por

A anotação `@requerido-por` é usada para indicar que um objeto ou ação é
necessário para outra coisa. Por exemplo, um script pode ter a anotação
`@requerido-por TABELA_A` para indicar que a execução desse script depende da
existência da tabela TABELA_A.

Exemplo de uso:

    -- @requerido-por TABELA_A

### @depende-de

A anotação `@depende-de` é usada para indicar que um objeto ou ação depende de outro objeto ou ação.
Por exemplo, um script pode ter a anotação `@depende-de TABELA_B` para indicar que a criação ou
atualização da tabela TABELA_B é necessária para a execução desse script.

Exemplo de uso:

    -- @depende-de TABELA_B

### @depende-apenas-de

A anotação `@depende-apenas-de` é usada para indicar que um objeto ou ação depende apenas de outro
objeto ou ação e não de outros objetos ou ações. Por exemplo, um script pode ter a anotação
`@depende-apenas-de TABELA_C` para indicar que apenas a criação ou atualização da tabela TABELA_C é
necessária para a execução desse script.

Exemplo de uso:

    -- @depende-apenas-de TABELA_C
