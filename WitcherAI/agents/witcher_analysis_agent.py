# Witcher Save Analysis Agent
# Autonomous AI agent for intelligent save file analysis across all Witcher games

from dataclasses import dataclass
from typing import Dict, List, Optional, Tuple
from enum import Enum
import sqlite3
import json
import logging

class AgentState(Enum):
    INITIALIZING = "initializing"
    ANALYZING = "analyzing" 
    LEARNING = "learning"
    PLANNING = "planning"
    EXECUTING = "executing"
    REFLECTING = "reflecting"

class AnalysisGoal(Enum):
    DISCOVER_PATTERNS = "discover_patterns"
    HUNT_DECISIONS = "hunt_decisions"
    VALIDATE_PATTERNS = "validate_patterns"
    CROSS_GAME_TRANSFER = "cross_game_transfer"
    PREDICT_ENDINGS = "predict_endings"

@dataclass
class AgentMemory:
    """Agent's working memory for analysis context"""
    current_game: str
    discovered_patterns: List[Dict]
    confidence_scores: Dict[str, float]
    failed_attempts: List[Dict]
    successful_strategies: List[Dict]
    cross_game_knowledge: Dict[str, List]

@dataclass
class AnalysisTask:
    """Autonomous task definition"""
    goal: AnalysisGoal
    save_file_path: str
    expected_patterns: List[str]
    priority: int
    context: Dict
    
class WitcherAnalysisAgent:
    """Autonomous AI agent for Witcher save file analysis"""
    
    def __init__(self, db_path: str, game_context: str):
        self.db_path = db_path
        self.game_context = game_context
        self.state = AgentState.INITIALIZING
        self.memory = AgentMemory(
            current_game=game_context,
            discovered_patterns=[],
            confidence_scores={},
            failed_attempts=[],
            successful_strategies=[],
            cross_game_knowledge={}
        )
        self.task_queue = []
        self.logger = logging.getLogger(f"WitcherAgent_{game_context}")
        
    def perceive_environment(self) -> Dict:
        """Analyze current context and available resources"""
        perception = {
            'available_saves': self.scan_save_files(),
            'database_state': self.assess_knowledge_base(),
            'previous_success_rate': self.calculate_success_metrics(),
            'cross_game_patterns': self.load_transferable_knowledge()
        }
        
        self.logger.info(f"Perceived environment: {len(perception['available_saves'])} saves available")
        return perception
    
    def reason_about_goals(self, perception: Dict) -> List[AnalysisTask]:
        """Autonomous goal setting and task planning"""
        tasks = []
        
        # Reason about what we need to discover
        if len(self.memory.discovered_patterns) < 10:
            # Need more pattern discovery
            tasks.append(AnalysisTask(
                goal=AnalysisGoal.DISCOVER_PATTERNS,
                save_file_path=self.select_optimal_save_for_discovery(perception),
                expected_patterns=[],
                priority=10,
                context={'strategy': 'broad_scan'}
            ))
        
        # Autonomous decision hunting strategy
        if self.needs_decision_variable_hunt():
            decision_targets = self.predict_decision_locations(perception)
            for target in decision_targets:
                tasks.append(AnalysisTask(
                    goal=AnalysisGoal.HUNT_DECISIONS,
                    save_file_path=target['save_path'],
                    expected_patterns=target['expected_decisions'],
                    priority=8,
                    context=target['context']
                ))
        
        # Cross-game learning opportunities
        if self.can_transfer_knowledge():
            tasks.append(AnalysisTask(
                goal=AnalysisGoal.CROSS_GAME_TRANSFER,
                save_file_path="",  # Multiple files
                expected_patterns=self.get_transferable_patterns(),
                priority=6,
                context={'source_games': ['witcher2']}
            ))
        
        return sorted(tasks, key=lambda t: t.priority, reverse=True)
    
    def select_optimal_save_for_discovery(self, perception: Dict) -> str:
        """Intelligently choose which save file to analyze"""
        saves = perception['available_saves']
        
        # Agent reasoning: larger saves likely have more decision data
        # But mid-size saves might have cleaner patterns
        optimal_size_range = (1000000, 2000000)  # 1-2MB sweet spot
        
        candidates = [s for s in saves if optimal_size_range[0] <= s['size'] <= optimal_size_range[1]]
        
        if not candidates:
            candidates = saves  # Fallback to any save
        
        # Choose save we haven't analyzed recently
        unanalyzed = [s for s in candidates if s['path'] not in [a['save_path'] for a in self.memory.failed_attempts]]
        
        return unanalyzed[0]['path'] if unanalyzed else candidates[0]['path']
    
    def execute_analysis_task(self, task: AnalysisTask) -> Dict:
        """Autonomously execute analysis with adaptive strategies"""
        self.state = AgentState.EXECUTING
        self.logger.info(f"Executing {task.goal.value} on {task.save_file_path}")
        
        if task.goal == AnalysisGoal.DISCOVER_PATTERNS:
            return self.autonomous_pattern_discovery(task)
        elif task.goal == AnalysisGoal.HUNT_DECISIONS:
            return self.autonomous_decision_hunting(task)
        elif task.goal == AnalysisGoal.CROSS_GAME_TRANSFER:
            return self.autonomous_transfer_learning(task)
        
        return {"status": "unknown_goal", "task": task}
    
    def autonomous_pattern_discovery(self, task: AnalysisTask) -> Dict:
        """Agent independently discovers patterns with adaptive extraction"""
        results = {"patterns_found": [], "confidence_scores": {}, "insights": []}
        
        # Agent decides extraction strategy based on previous success
        extraction_sizes = [4096, 8192, 16384, 32768]
        successful_size = self.get_most_successful_extraction_size()
        
        if successful_size:
            extraction_sizes = [successful_size] + [s for s in extraction_sizes if s != successful_size]
        
        for size in extraction_sizes[:2]:  # Try top 2 strategies
            try:
                # Execute enhanced DZIP analysis
                patterns = self.call_enhanced_dzip_analysis(task.save_file_path, size)
                
                if patterns and len(patterns) > len(results["patterns_found"]):
                    results["patterns_found"] = patterns
                    results["extraction_size_used"] = size
                    break  # Success - agent stops trying other sizes
                    
            except Exception as e:
                self.memory.failed_attempts.append({
                    "task": task,
                    "error": str(e),
                    "extraction_size": size
                })
        
        # Agent learns from results
        self.update_strategy_memory(task, results)
        return results
    
    def autonomous_decision_hunting(self, task: AnalysisTask) -> Dict:
        """Agent autonomously hunts for decision variables with learned strategies"""
        # Agent applies learned patterns from previous successful hunts
        hunting_strategy = self.get_optimal_hunting_strategy(task.context)
        
        results = self.call_decision_hunter(
            task.save_file_path, 
            hunting_strategy['extraction_size'],
            hunting_strategy['patterns']
        )
        
        # Agent evaluates success and adapts
        if results['decisions_found'] > 0:
            self.memory.successful_strategies.append({
                "context": task.context,
                "strategy": hunting_strategy,
                "success_count": results['decisions_found']
            })
        
        return results
    
    def reflect_and_learn(self, task_results: List[Dict]) -> Dict:
        """Agent reflection and learning from task outcomes"""
        self.state = AgentState.REFLECTING
        
        learning_insights = {
            "pattern_success_rate": self.calculate_pattern_success_rate(task_results),
            "optimal_strategies": self.identify_successful_strategies(),
            "knowledge_gaps": self.identify_knowledge_gaps(),
            "next_priorities": self.plan_next_investigation_targets()
        }
        
        # Agent updates its knowledge base autonomously
        self.update_reference_database(learning_insights)
        
        self.logger.info(f"Agent reflection complete. Success rate: {learning_insights['pattern_success_rate']:.2%}")
        return learning_insights
    
    def run_autonomous_analysis_cycle(self, max_iterations: int = 10) -> Dict:
        """Main autonomous agent loop"""
        self.logger.info("Starting autonomous analysis cycle")
        iteration = 0
        total_discoveries = 0
        
        while iteration < max_iterations:
            iteration += 1
            self.state = AgentState.PLANNING
            
            # Agent perceives and reasons autonomously
            perception = self.perceive_environment()
            tasks = self.reason_about_goals(perception)
            
            if not tasks:
                self.logger.info("Agent determined no more tasks needed")
                break
            
            # Execute highest priority task
            primary_task = tasks[0]
            results = self.execute_analysis_task(primary_task)
            
            # Agent learns and adapts
            learning = self.reflect_and_learn([results])
            total_discoveries += len(results.get('patterns_found', []))
            
            self.logger.info(f"Iteration {iteration}: {len(results.get('patterns_found', []))} new patterns discovered")
            
            # Agent decides if enough progress has been made
            if learning['pattern_success_rate'] < 0.1 and iteration > 3:
                self.logger.info("Agent determined diminishing returns - stopping early")
                break
        
        return {
            "iterations_completed": iteration,
            "total_patterns_discovered": total_discoveries,
            "final_knowledge_state": self.memory,
            "agent_recommendations": self.generate_recommendations()
        }

# Integration with existing WitcherCI framework
def create_witcher_analysis_agent(game: str) -> WitcherAnalysisAgent:
    """Factory function to create game-specific analysis agents"""
    return WitcherAnalysisAgent(
        db_path="database/witcher_save_manager.db",
        game_context=game
    )

# Example autonomous operation
if __name__ == "__main__":
    # Create autonomous agent for Witcher 2
    agent = create_witcher_analysis_agent("witcher2")
    
    # Agent runs completely autonomously
    results = agent.run_autonomous_analysis_cycle(max_iterations=5)
    
    print(f"Agent discovered {results['total_patterns_discovered']} patterns autonomously")
    print(f"Recommendations: {results['agent_recommendations']}")
