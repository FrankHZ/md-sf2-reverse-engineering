# L3 · 服务交互

**已确认：**服务先按当前位置和来访状态选择可用选项。付款、物品移交、复活、治愈、转职和存档有各自条件；取消或不符合条件时走各服务的拒绝或返回分支。

- **商店。** 购买要求可用商品、金币和能接收的成员。售出要求实际持有并满足可卖条件。一件物品是否进入特卖与之后的可购数量独立。参阅[选择与资源交换](../../synthesis/themes/progression-and-economy.md#源码静态服务交换)及[商店规则（英文源）](../../../contracts/party/service-interactions.md#shop-source-boundary)。
- **教堂。** 复活选择符合条件的倒下成员，按实际级别计费；治愈选择实际可清除的状态。费用不足或取消不完成交易。教堂保存和转职另有规则。参阅[转职](../../design-map.md#装备与转职)、[存档](../system/save-system.md)及[教堂规则（英文源）](../../../contracts/party/service-interactions.md#church-source-boundary)。
- **Caravan。** 出战编成与物品保管各自处理。放入、取回和交给他人要检查实际持有者、装备约束和接收容量。参阅[资源与容量](../../synthesis/themes/progression-and-economy.md#持有存储与出战成员身份)及[Caravan 服务（英文源）](../../../contracts/party/service-interactions.md#caravan-source-boundary)。
- **秘银。** 交付材料与付款、制作就绪、领取是不同阶段。领取需要能接收成品的持有者。参阅[秘银订单](../../synthesis/themes/progression-and-economy.md#秘银订单下单就绪与领取)及[铁匠规则（英文源）](../../../contracts/party/service-interactions.md#blacksmith-source-boundary)。
