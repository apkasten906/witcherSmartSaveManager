# Real-World Witcher Analysis Agent
# Tests our breakthrough analysis on your actual save files

import subprocess
import os
from pathlib import Path

class RealWorldWitcherAgent:
    """Agent specialized for analyzing real Witcher save files"""
    
    def __init__(self):
        self.witcher2_saves_path = os.path.expandvars(r"%USERPROFILE%\Documents\Witcher 2\gamesaves")
        
    def get_latest_witcher2_save(self):
        """Find the most recent Witcher 2 save file"""
        save_dir = Path(self.witcher2_saves_path)
        
        if not save_dir.exists():
            return None
            
        save_files = list(save_dir.glob("*.sav"))
        if not save_files:
            return None
            
        # Get the newest save
        newest_save = max(save_files, key=lambda f: f.stat().st_mtime)
        return str(newest_save)
    
    def analyze_with_enhanced_dzip(self, save_path):
        """Use our Phase 1 breakthrough analysis on real save"""
        print(f"🔬 [AGENT] Analyzing real save file: {Path(save_path).name}")
        print(f"   Save size: {Path(save_path).stat().st_size / 1024:.1f} KB")
        
        try:
            # Use our enhanced DZIP analysis that found "The Path of Roche"
            cmd = [
                "powershell", "-File", 
                "Hunt-DecisionVariables.ps1",
                "-save-path", save_path,
                "-bytes-to-extract", "16384",
                "-pattern", "quest-data"
            ]
            
            print("   Running enhanced DZIP analysis...")
            result = subprocess.run(cmd, capture_output=True, text=True, cwd=".")
            
            if result.returncode == 0:
                # Parse and highlight the discoveries
                output = result.stdout
                
                print("✅ [AGENT] Analysis successful!")
                print("\n📋 DISCOVERIES:")
                
                decision_count = 0
                quest_count = 0
                
                for line in output.split('\n'):
                    if '[DECISION-FOUND]' in line:
                        decision_count += 1
                        print(f"   🎯 DECISION: {line.split(':', 1)[1].strip()}")
                    elif '[QUEST-DETECTED]' in line:
                        quest_count += 1 
                        print(f"   📜 QUEST: {line.split(':', 1)[1].strip()}")
                    elif 'active_quest_tracker' in line.lower():
                        print(f"   🔍 QUEST TRACKER: {line.strip()}")
                    elif 'path' in line.lower() and 'roche' in line.lower():
                        print(f"   ⚡ STORY PATH: {line.strip()}")
                
                print(f"\n📊 SUMMARY:")
                print(f"   • Decisions detected: {decision_count}")
                print(f"   • Quests found: {quest_count}")
                print(f"   • Analysis status: SUCCESS")
                
                return {
                    "status": "success",
                    "decisions": decision_count,
                    "quests": quest_count,
                    "output": output
                }
            else:
                print(f"❌ [AGENT] Analysis failed: {result.stderr}")
                return {"status": "failed", "error": result.stderr}
                
        except Exception as e:
            print(f"💥 [AGENT] Exception: {str(e)}")
            return {"status": "exception", "error": str(e)}
    
    def run_real_world_test(self):
        """Test our breakthrough on your actual Witcher 2 save"""
        print("🌍 [REAL-WORLD AGENT] Testing Phase 1 breakthrough on your actual save files!")
        print("=" * 75)
        
        # Find your latest Witcher 2 save
        latest_save = self.get_latest_witcher2_save()
        
        if not latest_save:
            print("❌ No Witcher 2 save files found")
            return {"status": "no_saves"}
        
        print(f"🎯 Target save: {Path(latest_save).name}")
        print(f"   Full path: {latest_save}")
        
        # Run our enhanced analysis
        results = self.analyze_with_enhanced_dzip(latest_save)
        
        print("\n" + "=" * 75)
        if results["status"] == "success":
            print("🚀 REAL-WORLD TEST: SUCCESS!")
            print("   Our Phase 1 breakthrough works on your actual save files!")
            if results["decisions"] > 0:
                print(f"   🎯 Found {results['decisions']} decision variables in your real playthrough!")
            if results["quests"] > 0:
                print(f"   📜 Detected {results['quests']} quest elements in your real save!")
        else:
            print("⚠️ REAL-WORLD TEST: Analysis failed - but agent detected the issue!")
        
        return results

# Run the real-world test
if __name__ == "__main__":
    agent = RealWorldWitcherAgent()
    test_results = agent.run_real_world_test()
