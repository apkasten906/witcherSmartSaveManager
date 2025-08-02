# Cross-Game Discovery Agent - Real Save File Analysis
# Autonomously discovers and analyzes save files from Witcher 1, 2, and 3

import os
import sqlite3
import subprocess
import json
from pathlib import Path
import time
from dataclasses import dataclass
from typing import List, Dict, Optional

@dataclass
class GameConfig:
    name: str
    key: str
    save_path: str
    extension: str
    
@dataclass
class SaveDiscovery:
    game: str
    save_files: List[str]
    file_count: int
    total_size_mb: float
    newest_save: Optional[str]
    oldest_save: Optional[str]

class CrossGameDiscoveryAgent:
    """Autonomous agent that discovers and analyzes saves across all Witcher games"""
    
    def __init__(self, db_path: str = "database/witcher_save_manager.db"):
        self.db_path = db_path
        self.knowledge = {
            "games_discovered": {},
            "cross_game_patterns": [],
            "analysis_results": [],
            "transfer_learnings": []
        }
        
        # Game configurations from App.config
        self.game_configs = [
            GameConfig("Witcher 1", "Witcher1", 
                      os.path.expandvars(r"%USERPROFILE%\Documents\The Witcher\saves"),
                      "*.TheWitcherSave"),
            GameConfig("Witcher 2", "Witcher2",
                      os.path.expandvars(r"%USERPROFILE%\Documents\Witcher 2\gamesaves"), 
                      "*.sav"),
            GameConfig("Witcher 3", "Witcher3",
                      os.path.expandvars(r"%USERPROFILE%\Documents\The Witcher 3\gamesaves"),
                      "*.sav")
        ]
    
    def perceive_game_environment(self) -> Dict[str, SaveDiscovery]:
        """Agent autonomously discovers save files across all games"""
        print("🔍 [AGENT] Perceiving multi-game environment...")
        
        discoveries = {}
        
        for config in self.game_configs:
            print(f"   Scanning {config.name} at: {config.save_path}")
            
            if not Path(config.save_path).exists():
                print(f"   ❌ {config.name} save folder not found")
                discoveries[config.key] = SaveDiscovery(
                    game=config.name,
                    save_files=[],
                    file_count=0,
                    total_size_mb=0.0,
                    newest_save=None,
                    oldest_save=None
                )
                continue
            
            # Discover save files
            save_pattern = config.extension.replace("*", "")
            save_files = []
            total_size = 0
            
            try:
                for file_path in Path(config.save_path).glob(config.extension):
                    if file_path.is_file():
                        save_files.append(str(file_path))
                        total_size += file_path.stat().st_size
                
                # Sort by modification time
                save_files_with_time = [(f, Path(f).stat().st_mtime) for f in save_files]
                save_files_with_time.sort(key=lambda x: x[1])
                
                discovery = SaveDiscovery(
                    game=config.name,
                    save_files=[f[0] for f in save_files_with_time],
                    file_count=len(save_files),
                    total_size_mb=total_size / (1024 * 1024),
                    newest_save=save_files_with_time[-1][0] if save_files_with_time else None,
                    oldest_save=save_files_with_time[0][0] if save_files_with_time else None
                )
                
                discoveries[config.key] = discovery
                
                print(f"   ✅ {config.name}: {discovery.file_count} saves found ({discovery.total_size_mb:.1f}MB)")
                if discovery.newest_save:
                    print(f"      Latest: {Path(discovery.newest_save).name}")
                
            except Exception as e:
                print(f"   ⚠️ {config.name} scan failed: {str(e)}")
                discoveries[config.key] = SaveDiscovery(
                    game=config.name, save_files=[], file_count=0, 
                    total_size_mb=0.0, newest_save=None, oldest_save=None
                )
        
        return discoveries
    
    def decide_analysis_strategy(self, discoveries: Dict[str, SaveDiscovery]) -> Dict:
        """Agent reasons about optimal analysis strategy across games"""
        print("\n🧠 [AGENT] Reasoning about analysis strategy...")
        
        # Count available games
        available_games = [k for k, d in discoveries.items() if d.file_count > 0]
        total_saves = sum(d.file_count for d in discoveries.values())
        
        print(f"   Available games: {len(available_games)}")
        print(f"   Total save files: {total_saves}")
        
        if len(available_games) == 0:
            return {"action": "no_analysis", "reason": "no save files found"}
        
        if len(available_games) == 1:
            game_key = available_games[0]
            discovery = discoveries[game_key]
            return {
                "action": "single_game_analysis",
                "target_game": game_key,
                "target_save": discovery.newest_save,
                "reason": f"Only {discovery.game} available"
            }
        
        # Multiple games available - cross-game analysis!
        return {
            "action": "cross_game_analysis",
            "target_games": available_games,
            "primary_game": available_games[0],  # Start with first available
            "reason": f"Multiple games detected - cross-game learning possible"
        }
    
    def execute_cross_game_analysis(self, strategy: Dict, discoveries: Dict[str, SaveDiscovery]) -> Dict:
        """Agent executes intelligent cross-game analysis"""
        print(f"\n⚡ [AGENT] Executing {strategy['action']}...")
        
        if strategy["action"] == "no_analysis":
            return {"status": "no_action", "reason": strategy["reason"]}
        
        results = {"analyses": {}, "cross_game_insights": [], "status": "success"}
        
        if strategy["action"] == "single_game_analysis":
            # Analyze single game
            game_key = strategy["target_game"]
            target_save = strategy["target_save"]
            
            print(f"   Analyzing {game_key}: {Path(target_save).name}")
            
            analysis_result = self.analyze_single_save(target_save, game_key)
            results["analyses"][game_key] = analysis_result
            
        elif strategy["action"] == "cross_game_analysis":
            # Analyze each available game
            for game_key in strategy["target_games"]:
                discovery = discoveries[game_key]
                if discovery.newest_save:
                    print(f"   Analyzing {discovery.game}: {Path(discovery.newest_save).name}")
                    
                    analysis_result = self.analyze_single_save(discovery.newest_save, game_key)
                    results["analyses"][game_key] = analysis_result
            
            # Perform cross-game pattern matching
            cross_insights = self.find_cross_game_patterns(results["analyses"])
            results["cross_game_insights"] = cross_insights
        
        return results
    
    def analyze_single_save(self, save_path: str, game_key: str) -> Dict:
        """Analyze a single save file using our existing tools"""
        try:
            # Use our enhanced DZIP analysis for Witcher 2
            if game_key == "Witcher2":
                cmd = [
                    "powershell", "-File", 
                    "Hunt-DecisionVariables.ps1",
                    "-save-path", save_path,
                    "-bytes-to-extract", "16384"
                ]
            else:
                # For Witcher 1/3, use simpler hex analysis
                cmd = [
                    "powershell", "-File",
                    "Invoke-HexAnalysis.ps1", 
                    "-save-path", save_path,
                    "-output-format", "patterns"
                ]
            
            result = subprocess.run(cmd, capture_output=True, text=True, cwd=".")
            
            if result.returncode == 0:
                patterns = self.extract_patterns_from_output(result.stdout, game_key)
                
                return {
                    "status": "success",
                    "save_path": save_path,
                    "game": game_key,
                    "patterns_found": patterns,
                    "pattern_count": len(patterns),
                    "analysis_output": result.stdout[:1000]  # First 1KB for reference
                }
            else:
                return {
                    "status": "failed",
                    "save_path": save_path,
                    "game": game_key,
                    "error": result.stderr,
                    "patterns_found": [],
                    "pattern_count": 0
                }
                
        except Exception as e:
            return {
                "status": "exception",
                "save_path": save_path,
                "game": game_key,
                "error": str(e),
                "patterns_found": [],
                "pattern_count": 0
            }
    
    def extract_patterns_from_output(self, output: str, game_key: str) -> List[Dict]:
        """Extract meaningful patterns from analysis output"""
        patterns = []
        lines = output.split('\n')
        
        for line in lines:
            # Look for different pattern indicators
            if any(indicator in line.lower() for indicator in 
                   ["pattern", "quest", "decision", "variable", "state"]):
                
                # Extract pattern info
                if ":" in line:
                    parts = line.split(":", 1)
                    if len(parts) >= 2:
                        pattern_type = parts[0].strip()
                        pattern_value = parts[1].strip()
                        
                        patterns.append({
                            "type": pattern_type,
                            "value": pattern_value,
                            "game": game_key,
                            "confidence": 0.7  # Default confidence
                        })
        
        return patterns
    
    def find_cross_game_patterns(self, analyses: Dict) -> List[Dict]:
        """Agent identifies patterns that appear across multiple games"""
        print("   🔗 Searching for cross-game patterns...")
        
        cross_patterns = []
        
        # Get all patterns from all games
        all_patterns = {}
        for game, analysis in analyses.items():
            if analysis["status"] == "success":
                all_patterns[game] = analysis["patterns_found"]
        
        if len(all_patterns) < 2:
            return cross_patterns
        
        # Look for similar pattern types across games
        pattern_types = {}
        for game, patterns in all_patterns.items():
            for pattern in patterns:
                pattern_type = pattern["type"].lower()
                if pattern_type not in pattern_types:
                    pattern_types[pattern_type] = []
                pattern_types[pattern_type].append((game, pattern))
        
        # Find types that appear in multiple games
        for pattern_type, instances in pattern_types.items():
            if len(instances) >= 2:
                games_with_pattern = [inst[0] for inst in instances]
                cross_patterns.append({
                    "pattern_type": pattern_type,
                    "appears_in_games": games_with_pattern,
                    "instances": instances,
                    "transfer_potential": "high" if len(games_with_pattern) >= 3 else "medium"
                })
                
                print(f"      Found '{pattern_type}' in: {', '.join(games_with_pattern)}")
        
        return cross_patterns
    
    def learn_and_adapt(self, results: Dict) -> Dict:
        """Agent learns from cross-game analysis results"""
        print("\n🎓 [AGENT] Learning from cross-game analysis...")
        
        learning_summary = {
            "games_analyzed": len(results["analyses"]),
            "total_patterns": sum(a.get("pattern_count", 0) for a in results["analyses"].values()),
            "cross_game_patterns": len(results.get("cross_game_insights", [])),
            "insights": []
        }
        
        # Store knowledge for future use
        self.knowledge["analysis_results"].append(results)
        
        # Generate insights
        if learning_summary["cross_game_patterns"] > 0:
            learning_summary["insights"].append(
                f"Discovered {learning_summary['cross_game_patterns']} pattern types shared across games"
            )
        
        if learning_summary["total_patterns"] > 10:
            learning_summary["insights"].append(
                "High pattern density suggests rich save file structure"
            )
        
        # Store patterns in database for future reference
        try:
            self.store_patterns_in_database(results)
            learning_summary["insights"].append("Patterns stored in knowledge database")
        except Exception as e:
            print(f"   ⚠️ Database storage failed: {str(e)}")
        
        print(f"   📊 Analysis summary: {learning_summary['games_analyzed']} games, {learning_summary['total_patterns']} patterns")
        
        return learning_summary
    
    def store_patterns_in_database(self, results: Dict):
        """Store discovered patterns in the knowledge database"""
        if not Path(self.db_path).exists():
            print(f"   ⚠️ Database not found: {self.db_path}")
            return
        
        conn = sqlite3.connect(self.db_path)
        try:
            cursor = conn.cursor()
            
            for game, analysis in results["analyses"].items():
                if analysis["status"] == "success":
                    for pattern in analysis["patterns_found"]:
                        cursor.execute("""
                            INSERT INTO PatternGameMapping 
                            (pattern_text, pattern_type, game_concept, confidence_level, data_type, verification_status)
                            VALUES (?, ?, ?, ?, ?, ?)
                        """, (
                            pattern["value"],
                            pattern["type"], 
                            f"{game}_discovery",
                            pattern["confidence"],
                            "auto_discovered",
                            "agent_found"
                        ))
            
            conn.commit()
            print("   ✅ Patterns stored in knowledge database")
            
        finally:
            conn.close()
    
    def run_autonomous_discovery(self) -> Dict:
        """Run complete autonomous cross-game discovery and analysis"""
        print("🚀 [CROSS-GAME AGENT] Starting autonomous discovery across all Witcher games...")
        print("=" * 70)
        
        # Agent perception phase
        discoveries = self.perceive_game_environment()
        
        # Agent decision phase  
        strategy = self.decide_analysis_strategy(discoveries)
        
        # Agent execution phase
        results = self.execute_cross_game_analysis(strategy, discoveries)
        
        # Agent learning phase
        learning = self.learn_and_adapt(results)
        
        # Generate final report
        print("\n" + "=" * 70)
        print("🎯 [CROSS-GAME AGENT] Autonomous discovery complete!")
        print(f"   Strategy: {strategy['action']}")
        print(f"   Games discovered: {', '.join(k for k, d in discoveries.items() if d.file_count > 0)}")
        print(f"   Total patterns found: {learning['total_patterns']}")
        print(f"   Cross-game patterns: {learning['cross_game_patterns']}")
        
        if learning["insights"]:
            print("   Key insights:")
            for insight in learning["insights"]:
                print(f"      • {insight}")
        
        return {
            "discoveries": discoveries,
            "strategy": strategy,
            "results": results,
            "learning": learning,
            "agent_status": "autonomous_success"
        }

# Test the cross-game discovery agent
if __name__ == "__main__":
    print("🌟 Testing Cross-Game Discovery Agent with Real Save Files")
    print("This agent will autonomously discover and analyze your Witcher saves!\n")
    
    agent = CrossGameDiscoveryAgent()
    final_results = agent.run_autonomous_discovery()
    
    print(f"\n📈 Final Agent Knowledge State:")
    print(f"   Total discovery sessions: {len(agent.knowledge['analysis_results'])}")
    print(f"   Games in knowledge base: {len(agent.knowledge['games_discovered'])}")
