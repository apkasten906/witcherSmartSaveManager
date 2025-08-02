#!/usr/bin/env python3
"""
🎯 Phase 2B: Universal Decision Taxonomy System
==============================================
Implements a unified decision classification system across all Witcher games
"""

import pandas as pd
import numpy as np
from dataclasses import dataclass
from typing import Dict, List, Optional
from pathlib import Path

@dataclass
class DecisionNode:
    """Universal decision classification node"""
    id: str
    category: str
    subcategory: str
    description: str
    games: List[str]
    impact_level: str  # 'minor', 'major', 'critical'
    confidence: float
    patterns: List[str]

class UniversalDecisionTaxonomy:
    """
    Universal Decision Taxonomy System for Witcher Games
    
    Classifies all decisions into a unified hierarchy that works
    across Witcher 1, 2, and 3 save files.
    """
    
    def __init__(self):
        self.decision_tree = {}
        self.categories = {
            'character': 'Character Relationships',
            'political': 'Political Alignment', 
            'moral': 'Moral Choices',
            'quest': 'Quest Progression',
            'romance': 'Romance Options',
            'combat': 'Combat Style',
            'inventory': 'Equipment Choices'
        }
        self._build_taxonomy()
    
    def _build_taxonomy(self):
        """Build the universal decision taxonomy"""
        
        # Character Relationship Decisions
        self.decision_tree['character'] = [
            DecisionNode(
                id='char_loyalty_path',
                category='character',
                subcategory='loyalty',
                description='Character loyalty path choices',
                games=['Witcher 1', 'Witcher 2', 'Witcher 3'],
                impact_level='critical',
                confidence=0.94,
                patterns=['roche_path', 'iorveth_path', 'triss_choice', 'yennefer_choice']
            ),
            DecisionNode(
                id='char_companion_fate',
                category='character',
                subcategory='fate',
                description='Companion survival and outcomes',
                games=['Witcher 1', 'Witcher 2', 'Witcher 3'],
                impact_level='major',
                confidence=0.88,
                patterns=['companion_alive', 'character_state', 'relationship_status']
            )
        ]
        
        # Political Alignment Decisions
        self.decision_tree['political'] = [
            DecisionNode(
                id='pol_faction_choice',
                category='political',
                subcategory='faction',
                description='Political faction alignment',
                games=['Witcher 2', 'Witcher 3'],
                impact_level='critical',
                confidence=0.91,
                patterns=['political_stance', 'faction_alignment', 'kingdom_choice']
            ),
            DecisionNode(
                id='pol_ruler_support',
                category='political',
                subcategory='leadership',
                description='Support for rulers and leaders',
                games=['Witcher 1', 'Witcher 2', 'Witcher 3'],
                impact_level='major',
                confidence=0.83,
                patterns=['ruler_choice', 'political_support', 'crown_decision']
            )
        ]
        
        # Moral Choice Decisions
        self.decision_tree['moral'] = [
            DecisionNode(
                id='mor_life_death',
                category='moral',
                subcategory='life_death',
                description='Life and death moral choices',
                games=['Witcher 1', 'Witcher 2', 'Witcher 3'],
                impact_level='critical',
                confidence=0.89,
                patterns=['kill_choice', 'spare_choice', 'execution_decision']
            ),
            DecisionNode(
                id='mor_justice_mercy',
                category='moral',
                subcategory='justice',
                description='Justice vs mercy decisions',
                games=['Witcher 1', 'Witcher 2', 'Witcher 3'],
                impact_level='major',
                confidence=0.85,
                patterns=['justice_choice', 'mercy_decision', 'punishment_type']
            )
        ]
        
        # Quest Progression Decisions
        self.decision_tree['quest'] = [
            DecisionNode(
                id='que_main_path',
                category='quest',
                subcategory='main_story',
                description='Main storyline progression choices',
                games=['Witcher 1', 'Witcher 2', 'Witcher 3'],
                impact_level='critical',
                confidence=0.96,
                patterns=['questSystem', 'active_quest_tracker', 'story_progress']
            ),
            DecisionNode(
                id='que_side_completion',
                category='quest',
                subcategory='side_quests',
                description='Side quest completion and choices',
                games=['Witcher 1', 'Witcher 2', 'Witcher 3'],
                impact_level='minor',
                confidence=0.78,
                patterns=['side_quest_state', 'optional_objectives', 'quest_outcome']
            )
        ]
    
    def classify_decision(self, pattern: str, context: Dict) -> Optional[DecisionNode]:
        """Classify a decision pattern using the taxonomy"""
        
        best_match = None
        best_score = 0.0
        
        for category, decisions in self.decision_tree.items():
            for decision in decisions:
                # Calculate similarity score
                score = 0.0
                
                # Pattern matching
                if pattern in decision.patterns:
                    score += 0.4
                
                # Partial pattern matching
                for dp in decision.patterns:
                    if dp in pattern or pattern in dp:
                        score += 0.2
                
                # Context relevance
                if 'game' in context and context['game'] in decision.games:
                    score += 0.2
                
                # Confidence weighting
                score *= decision.confidence
                
                if score > best_score:
                    best_score = score
                    best_match = decision
        
        return best_match if best_score > 0.3 else None
    
    def get_cross_game_mappings(self) -> Dict[str, List[str]]:
        """Get patterns that map across multiple games"""
        
        mappings = {}
        
        for category, decisions in self.decision_tree.items():
            for decision in decisions:
                if len(decision.games) > 1:  # Cross-game decision
                    key = f"{decision.category}_{decision.subcategory}"
                    mappings[key] = {
                        'patterns': decision.patterns,
                        'games': decision.games,
                        'impact': decision.impact_level,
                        'confidence': decision.confidence
                    }
        
        return mappings
    
    def analyze_save_decisions(self, patterns: List[str], game: str) -> Dict:
        """Analyze decision patterns found in a save file"""
        
        results = {
            'total_patterns': len(patterns),
            'classified_decisions': [],
            'unclassified_patterns': [],
            'decision_summary': {}
        }
        
        for pattern in patterns:
            context = {'game': game}
            decision = self.classify_decision(pattern, context)
            
            if decision:
                results['classified_decisions'].append({
                    'pattern': pattern,
                    'decision': decision,
                    'category': decision.category,
                    'impact': decision.impact_level
                })
            else:
                results['unclassified_patterns'].append(pattern)
        
        # Generate summary by category
        for item in results['classified_decisions']:
            category = item['category']
            if category not in results['decision_summary']:
                results['decision_summary'][category] = {
                    'count': 0,
                    'critical': 0,
                    'major': 0,
                    'minor': 0
                }
            
            results['decision_summary'][category]['count'] += 1
            results['decision_summary'][category][item['impact']] += 1
        
        return results

def demonstrate_taxonomy():
    """Demonstrate the Universal Decision Taxonomy system"""
    
    print("🎯 Universal Decision Taxonomy System")
    print("=" * 50)
    
    taxonomy = UniversalDecisionTaxonomy()
    
    print("📊 Decision Categories:")
    for cat_id, cat_name in taxonomy.categories.items():
        decisions = taxonomy.decision_tree.get(cat_id, [])
        print(f"  • {cat_name}: {len(decisions)} decision types")
    
    print("\n🔍 Cross-Game Decision Mappings:")
    mappings = taxonomy.get_cross_game_mappings()
    
    for decision_type, mapping in mappings.items():
        print(f"\n  {decision_type}:")
        print(f"    Games: {', '.join(mapping['games'])}")
        print(f"    Impact: {mapping['impact']}")
        print(f"    Confidence: {mapping['confidence']:.3f}")
        print(f"    Patterns: {', '.join(mapping['patterns'][:3])}...")
    
    print("\n🧪 Testing Pattern Classification:")
    test_patterns = [
        'roche_path_confirmed',
        'questSystem',
        'political_stance',
        'character_alive',
        'unknown_pattern'
    ]
    
    for pattern in test_patterns:
        decision = taxonomy.classify_decision(pattern, {'game': 'Witcher 2'})
        if decision:
            print(f"  ✅ {pattern} → {decision.category}.{decision.subcategory} ({decision.confidence:.3f})")
        else:
            print(f"  ❓ {pattern} → Unclassified")
    
    print("\n🎯 Sample Save Analysis:")
    sample_patterns = ['roche_path_confirmed', 'questSystem', 'political_stance', 'companion_alive']
    analysis = taxonomy.analyze_save_decisions(sample_patterns, 'Witcher 2')
    
    print(f"  Total patterns: {analysis['total_patterns']}")
    print(f"  Classified: {len(analysis['classified_decisions'])}")
    print(f"  Unclassified: {len(analysis['unclassified_patterns'])}")
    
    print("\n  Decision Summary by Category:")
    for category, summary in analysis['decision_summary'].items():
        print(f"    {category}: {summary['count']} decisions")
        print(f"      Critical: {summary['critical']}, Major: {summary['major']}, Minor: {summary['minor']}")
    
    print("\n✅ Universal Decision Taxonomy: OPERATIONAL!")
    print("🚀 Ready for Knowledge Transfer Engine implementation")

if __name__ == "__main__":
    demonstrate_taxonomy()
