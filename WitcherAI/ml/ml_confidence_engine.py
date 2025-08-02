# ML Pattern Confidence Engine
# Uses machine learning to automatically score pattern reliability

import sqlite3
import numpy as np
from sklearn.ensemble import RandomForestClassifier
from sklearn.feature_extraction.text import TfidfVectorizer
import json

class PatternConfidenceEngine:
    def __init__(self, db_path):
        self.db_path = db_path
        self.model = RandomForestClassifier(n_estimators=100)
        self.vectorizer = TfidfVectorizer(max_features=1000)
        
    def extract_pattern_features(self, pattern_text, context, frequency):
        """Extract features for ML confidence scoring"""
        features = {
            'pattern_length': len(pattern_text),
            'has_underscore': '_' in pattern_text,
            'frequency_score': frequency,
            'context_relevance': self.calculate_context_relevance(pattern_text, context),
            'game_keyword_match': self.count_game_keywords(pattern_text),
            'structural_indicators': self.detect_structural_patterns(pattern_text)
        }
        return features
    
    def calculate_context_relevance(self, pattern, context):
        """Score how well pattern matches expected game context"""
        game_keywords = ['quest', 'character', 'decision', 'act', 'choice', 'fact']
        context_lower = context.lower()
        return sum(1 for keyword in game_keywords if keyword in context_lower)
    
    def count_game_keywords(self, pattern):
        """Count game-specific patterns in the text"""
        witcher_patterns = ['quest', 'aryan', 'roche', 'iorveth', 'facts', 'choice']
        pattern_lower = pattern.lower()
        return sum(1 for wp in witcher_patterns if wp in pattern_lower)
    
    def detect_structural_patterns(self, pattern):
        """Detect structural indicators of valid game data"""
        indicators = 0
        if pattern.isupper(): indicators += 1  # Constants like SAVY, BLCK
        if 'System' in pattern: indicators += 1  # Game systems
        if len(pattern) == 4 and pattern.isupper(): indicators += 2  # DZIP headers
        return indicators
    
    def train_confidence_model(self):
        """Train ML model on existing verified patterns"""
        conn = sqlite3.connect(self.db_path)
        
        # Get training data from verified patterns
        verified_patterns = conn.execute("""
            SELECT pattern_text, context_clues, confidence_level, verification_status
            FROM PatternGameMapping 
            WHERE verification_status IN ('confirmed', 'pending')
        """).fetchall()
        
        features = []
        labels = []
        
        for pattern, context, confidence, status in verified_patterns:
            # Create synthetic frequency data for training
            frequency = np.random.randint(1, 10)
            feature_dict = self.extract_pattern_features(pattern, context, frequency)
            features.append(list(feature_dict.values()))
            
            # Convert confidence to binary classification (high/low)
            labels.append(1 if confidence >= 0.85 else 0)
        
        if len(features) > 0:
            self.model.fit(features, labels)
            print(f"Trained confidence model on {len(features)} verified patterns")
        
        conn.close()
    
    def predict_confidence(self, pattern_text, context, frequency):
        """Predict confidence score for new pattern"""
        features = self.extract_pattern_features(pattern_text, context, frequency)
        feature_vector = np.array(list(features.values())).reshape(1, -1)
        
        # Get probability of high confidence
        confidence_prob = self.model.predict_proba(feature_vector)[0][1]
        
        # Convert to 0.5-0.95 range for game analysis
        return 0.5 + (confidence_prob * 0.45)
    
    def auto_score_new_patterns(self, discovered_patterns):
        """Automatically score newly discovered patterns"""
        scored_patterns = []
        
        for pattern_data in discovered_patterns:
            predicted_confidence = self.predict_confidence(
                pattern_data['pattern'],
                pattern_data.get('context', ''),
                pattern_data.get('frequency', 1)
            )
            
            pattern_data['ml_confidence'] = predicted_confidence
            pattern_data['verification_status'] = 'ml_pending'
            scored_patterns.append(pattern_data)
        
        return scored_patterns

# Usage for Witcher save analysis
def integrate_ml_confidence():
    engine = PatternConfidenceEngine('database/witcher_save_manager.db')
    engine.train_confidence_model()
    
    # This would integrate with our DZIP analysis
    return engine
