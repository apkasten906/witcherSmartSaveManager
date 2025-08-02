# Simple Autonomous Agent - Proof of Concept
# This agent autonomously analyzes one save file and learns from the results

import subprocess
import json
import sqlite3
from pathlib import Path
import time

class SimpleWitcherAgent:
    """Minimal autonomous agent to prove the agentic concept"""
    
    def __init__(self, db_path: str):
        self.db_path = db_path
        self.knowledge = {"patterns_seen": [], "successful_strategies": [], "failures": []}
        self.current_goal = "discover_new_patterns"
        
    def perceive_environment(self):
        """Agent perceives what save files are available"""
        save_dir = Path("savesAnalysis/_backup")
        
        if not save_dir.exists():
            print("[AGENT] No save directory found - creating test environment")
            return {"available_saves": [], "status": "no_saves"}
        
        saves = list(save_dir.glob("*.sav"))
        
        # Agent reasons about optimal save selection
        if saves:
            # Prefer saves we haven't analyzed yet
            unanalyzed = [s for s in saves if str(s) not in [f["save_path"] for f in self.knowledge["failures"]]]
            target_save = unanalyzed[0] if unanalyzed else saves[0]
            
            print(f"[AGENT] Perceiving environment: {len(saves)} saves available")
            print(f"[AGENT] Selected target: {target_save.name}")
            
            return {
                "available_saves": saves,
                "target_save": str(target_save),
                "status": "ready"
            }
        
        return {"available_saves": [], "status": "no_saves"}
    
    def decide_strategy(self, perception):
        """Agent autonomously decides analysis strategy"""
        if perception["status"] != "ready":
            return {"action": "wait", "reason": "no saves available"}
        
        # Agent learns from previous attempts
        if len(self.knowledge["successful_strategies"]) > 0:
            # Use most successful strategy
            best_strategy = max(self.knowledge["successful_strategies"], 
                              key=lambda x: x["success_score"])
            print(f"[AGENT] Using learned successful strategy: {best_strategy['strategy_name']}")
            return {
                "action": "analyze_with_learned_strategy",
                "strategy": best_strategy,
                "target": perception["target_save"]
            }
        else:
            # First attempt - try conservative approach
            print("[AGENT] First analysis - using conservative strategy")
            return {
                "action": "analyze_conservatively", 
                "target": perception["target_save"],
                "bytes_to_extract": 8192,
                "strategy_name": "conservative_exploration"
            }
    
    def execute_analysis(self, decision):
        """Agent executes the analysis autonomously"""
        if decision["action"] == "wait":
            print("[AGENT] Waiting - no action possible")
            return {"status": "waiting"}
        
        target_save = decision["target"]
        bytes_to_extract = decision.get("bytes_to_extract", 8192)
        
        print(f"[AGENT] Executing analysis on {Path(target_save).name}")
        print(f"[AGENT] Strategy: {decision.get('strategy_name', 'unknown')}")
        
        try:
            # Agent calls our existing enhanced analysis tool
            cmd = [
                "powershell", "-File", 
                "tools/witcherci/scripts/Hunt-DecisionVariables.ps1",
                "-save-path", target_save,
                "-bytes-to-extract", str(bytes_to_extract)
            ]
            
            result = subprocess.run(cmd, capture_output=True, text=True, cwd=".")
            
            if result.returncode == 0:
                # Parse the results to extract discoveries
                output = result.stdout
                decisions_found = self.parse_decision_output(output)
                
                success_result = {
                    "status": "success",
                    "decisions_found": decisions_found,
                    "save_analyzed": target_save,
                    "strategy_used": decision.get("strategy_name"),
                    "output": output
                }
                
                print(f"[AGENT] SUCCESS! Found {len(decisions_found)} decision patterns")
                return success_result
            else:
                error_result = {
                    "status": "failed",
                    "error": result.stderr,
                    "save_path": target_save,
                    "strategy_used": decision.get("strategy_name")
                }
                print(f"[AGENT] FAILED: {result.stderr}")
                return error_result
                
        except Exception as e:
            print(f"[AGENT] EXCEPTION: {str(e)}")
            return {"status": "exception", "error": str(e)}
    
    def parse_decision_output(self, output):
        """Extract decision discoveries from tool output"""
        decisions = []
        lines = output.split('\n')
        
        for line in lines:
            if "[DECISION-FOUND]" in line:
                # Extract pattern name and context
                parts = line.split(": ")
                if len(parts) >= 2:
                    pattern_info = parts[1].split(" matches - ")
                    if len(pattern_info) >= 2:
                        pattern_name = pattern_info[0].split(" (")[0]
                        context = pattern_info[1] if len(pattern_info) > 1 else "unknown"
                        decisions.append({
                            "pattern": pattern_name,
                            "context": context,
                            "source": "decision_hunter"
                        })
        
        return decisions
    
    def learn_from_results(self, decision, results):
        """Agent learns and updates its knowledge"""
        if results["status"] == "success":
            # Learn successful strategy
            strategy_record = {
                "strategy_name": decision.get("strategy_name", "unknown"),
                "success_score": len(results["decisions_found"]),
                "save_size": Path(results["save_analyzed"]).stat().st_size if Path(results["save_analyzed"]).exists() else 0,
                "timestamp": time.time()
            }
            
            self.knowledge["successful_strategies"].append(strategy_record)
            
            # Store discovered patterns
            for decision_pattern in results["decisions_found"]:
                if decision_pattern not in self.knowledge["patterns_seen"]:
                    self.knowledge["patterns_seen"].append(decision_pattern)
            
            print(f"[AGENT] LEARNED: Strategy '{decision.get('strategy_name')}' successful with score {strategy_record['success_score']}")
            
        else:
            # Learn from failure
            failure_record = {
                "strategy_name": decision.get("strategy_name", "unknown"),
                "save_path": results.get("save_path", "unknown"),
                "error": results.get("error", "unknown"),
                "timestamp": time.time()
            }
            
            self.knowledge["failures"].append(failure_record)
            print(f"[AGENT] LEARNED: Strategy '{decision.get('strategy_name')}' failed - will avoid")
    
    def run_autonomous_cycle(self, max_iterations=3):
        """Agent runs autonomously for multiple cycles"""
        print("🤖 [AGENT] Starting autonomous analysis cycle...")
        
        for iteration in range(max_iterations):
            print(f"\n--- ITERATION {iteration + 1} ---")
            
            # Agent perceives, decides, acts, and learns
            perception = self.perceive_environment()
            decision = self.decide_strategy(perception)
            results = self.execute_analysis(decision)
            self.learn_from_results(decision, results)
            
            # Agent reflects on progress
            total_patterns = len(self.knowledge["patterns_seen"])
            success_rate = len(self.knowledge["successful_strategies"]) / (len(self.knowledge["successful_strategies"]) + len(self.knowledge["failures"])) if (len(self.knowledge["successful_strategies"]) + len(self.knowledge["failures"])) > 0 else 0
            
            print(f"[AGENT] Progress: {total_patterns} patterns discovered, {success_rate:.2%} success rate")
            
            # Agent decides if it should continue
            if results["status"] == "success" and len(results.get("decisions_found", [])) == 0:
                print("[AGENT] No new discoveries - may need different approach")
            
            time.sleep(1)  # Brief pause between iterations
        
        print(f"\n🎯 [AGENT] Autonomous cycle complete!")
        print(f"   Total patterns discovered: {len(self.knowledge['patterns_seen'])}")
        print(f"   Successful strategies: {len(self.knowledge['successful_strategies'])}")
        print(f"   Failed attempts: {len(self.knowledge['failures'])}")
        
        return self.knowledge

# Test the autonomous agent
if __name__ == "__main__":
    print("🚀 Testing Simple Autonomous Witcher Analysis Agent")
    
    agent = SimpleWitcherAgent("database/witcher_save_manager.db")
    results = agent.run_autonomous_cycle(max_iterations=2)
    
    print("\n📊 Agent Learning Summary:")
    for pattern in results["patterns_seen"]:
        print(f"   - {pattern['pattern']}: {pattern['context']}")
