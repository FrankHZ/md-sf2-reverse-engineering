# 文档索引

这是中文导航入口。[英文索引](./README.md)是当前规范索引；详细证据、合同和决策以各自的英文所有者为准。
设计文档翻译遵循[术语表](./design/glossary.md)，本入口不复制覆盖率、案例数量或历史验收清单。

先检查 Git 提交、工作树、改动和进程状态，再沿[任务恢复入口](./operations/agent-resume.md)查找最近的文档。
常规任务无需通读全局索引或覆盖率台账。仓库文档和精确 Git 对象是持久记录；压缩后的旧聊天与历史启动脚本不构成新指令。

| 工作 | 当前入口 |
| --- | --- |
| 工作规则、隔离与验证范围 | [AGENTS](../AGENTS.md) |
| 引擎实现与架构 | [Remake README](../remake/README.md)、[架构](../remake/docs/architecture.md) |
| 新设计与测试政策 | [ADR 0019](./decisions/0019-state-and-content-driven-remake-engine.md) |
| 当前可运行能力与限制 | [能力矩阵](../remake/docs/capability-status.md) |
| 验证命令、Godot 实例复用 | [开发与验证](../remake/docs/development-and-verification.md) |
| 原版研究与证据前沿 | [Research 索引](./README.md#research)、[覆盖率所有者](./research/source-coverage.md) |
| 行为合同与综合设计 | [Design 索引](./README.md#design) |
| 原始决策和后续修订 | [Decisions 索引](./README.md#decisions) |
| 文档与 agent instruction 审计 | [审计记录](./operations/documentation-agent-audit.md) |

ADR 0019 的引擎迁移仍为 Proposed，文档合并没有实施 M0/M1、切换 CI 或完成引擎。
用户已确定的测试政策现在生效：只增加实际引擎行为的 unit tests；验证、probe、fixture driver、planner、gate、报告和助手本身不再另加测试。
旧测试按行为迁移或废弃，不要求数量保留、每删必补或旧全套全绿。原版证据保留，8C/H4 目标仍未完成。

Godot 默认复用已有安装、项目和运行中的调试实例。行为验收读取实际状态与输入结果，禁止截图验收。
只有具体故障或必要的启动、导入、导出、清理验证才另行启动或隔离实例。
