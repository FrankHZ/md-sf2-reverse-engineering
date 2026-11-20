# L3 · Caravan 保管与商店特卖

物品保管、持有和装备分别产生后果。可保管的物品移出当前携带栏，取回需有接收空间。商店特卖中的可购数量随符合特卖规则的交易变化；同一件物品的装备效果仍由实际持有和穿戴决定。参阅[成长与经济](../../synthesis/themes/progression-and-economy.md#金币与物品经济边界)、[本地服务](service-interactions.md)及[Caravan 与特卖依据（英文源）](../../../contracts/party/caravan-and-deals-state.md)。

Caravan 菜单守卫限制保管 64 件，成员各有四格。取回、交给他人与交换需要检查接收方，背包满不等于可无条件交换。存入后不保留装备等携带状态。符合特卖资格的珍稀物品按件计数，每种上限 15；买走一件减少一件，同一家已在普通目录出售的同种物品不再列入当地特卖。具体服务拒绝和持久化以[资源交换](../../synthesis/themes/progression-and-economy.md#源码静态服务交换)为准。
